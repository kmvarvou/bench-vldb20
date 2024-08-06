using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using TestingFramework.AlgoIntegration;

namespace TestingFramework.Testing
{
    public static class TestRoutines
    {
        #region Configuration

        private const int RandomSeed = 18931;
        
        private static (ValueTuple<int, int, int>[], int[]) GetExperimentSetup(ExperimentType et, ExperimentScenario es,
            int rows, int columns, String code)
        {
            int blockSize;
            int startOffset;
            int stepSize;
            
            switch (et)
            {
                case ExperimentType.Continuous:
                    switch (es)
                    {
                        // single-column
                        case ExperimentScenario.Missing:
                            blockSize = rows / 10;
                            //Console.Write("AUTO EINAI");
                            return (new[] {(0, -1, -1)}, Utils.ClosedSequence(blockSize, blockSize * 8, blockSize).ToArray());
                        
                        case ExperimentScenario.Length:
                            blockSize = rows / 10;
                            stepSize = rows / 10;
                            return (new[] {(0, -1, blockSize)}, Utils.ClosedSequence(2 * stepSize, rows, stepSize).ToArray());
                        
                        case ExperimentScenario.Columns:
                            blockSize = rows / 10;
                            stepSize = columns / 10;
                            return (new[] {(0, rows - blockSize, blockSize)}, Utils.ClosedSequence(
                                stepSize >= AlgoPack.TypicalTruncation + 1 ? stepSize : AlgoPack.TypicalTruncation + 1, columns, stepSize
                            ).ToArray());
                        
                        // full
                        case ExperimentScenario.Fullrow:
                            return (new[] {(-1, -1, -1)}, Utils.ClosedSequence(10, 100, 10).TakeWhile(x => x + 10 < rows).ToArray());

                        default:
                            throw new ArgumentException("Unrecognized experiment scenario or an incompatible combination with experiment type");
                    }
                    
                case ExperimentType.Recovery:
                    switch (es)
                    {
                        // single-column
                        case ExperimentScenario.Missing:
                            blockSize = rows / 10;
                            //startOffset = rows / 20;
                            Random r = new Random();
                            startOffset= r.Next(0, rows-(4*blockSize)-1);
                            return (new[] {(0, startOffset, -1)}, Utils.ClosedSequence(blockSize, blockSize*4, blockSize).ToArray());
                            
                        case ExperimentScenario.Length:
                            blockSize = rows / 10;
                            stepSize = rows / 10;
                            startOffset = rows / 20;
                            return (new[] {(0, startOffset, blockSize)}, Utils.ClosedSequence(2 * stepSize, rows, stepSize).ToArray());
                            
                        case ExperimentScenario.Columns:
                            blockSize = rows / 10;
                            stepSize = columns / 10;
                            startOffset = rows / 20;
                            return (new[] {(0, startOffset, blockSize)}, Utils.ClosedSequence(stepSize >= 4 ? stepSize : 4, columns, stepSize).ToArray());
                            
                        // multi-column
                        case ExperimentScenario.MultiColumnDisjoint:
                            stepSize = columns / 10;
                            return (new[] {(-1, -1, -1)}, Utils.ClosedSequence(stepSize, stepSize * 10, stepSize).ToArray());//stepsize * 10
                            
                        case ExperimentScenario.MulticolumnOverlap:
                            stepSize = columns / 10;
                            return (new[] {(-1, -1, -1)}, Utils.ClosedSequence(stepSize, stepSize * 10, stepSize).ToArray());
                        
                        case ExperimentScenario.MissingSubMatrix:
                            return (new[] {(-1, -1, -1)}, Utils.ClosedSequence(90, 100, 10).ToArray());
                        
                        // full
                        case ExperimentScenario.Fullrow:
                            return (new[] {(-1, -1, -1)}, Utils.ClosedSequence(10, 100, 10).TakeWhile(x => (x + rows / 20) < rows).ToArray());
                        
                        case ExperimentScenario.Fullcolumn:
                            return (new[] {(-1, -1, -1)}, Utils.ClosedSequence(1, 10).TakeWhile(x => x < columns - AlgoPack.TypicalTruncation).ToArray());

                        default:
                            throw new ArgumentException("Unrecognized experiment scenario");
                    }
                
                case ExperimentType.Streaming:
                    switch (es)
                    {
                        default:
                            throw new ArgumentException("Unrecognized experiment scenario");
                    }
                
                default:
                    throw new ArgumentException("Unrecognized experiment type");
            }
        }

        private static void UpdateMissingBlocks(ExperimentType et, ExperimentScenario es, int rows,
            int tcase, ref ValueTuple<int, int, int>[] missingBlocks, int columns, int code, String code2)
        {
            int MulticolBlockSize = rows / columns;
            if(MulticolBlockSize==0)
            {
              MulticolBlockSize = rows /rows;
              
              
            }
            if(MulticolBlockSize==0)
            {
              MulticolBlockSize = 1;
              
              
            }
            Console.WriteLine("here2");
            Console.WriteLine(tcase);
            Console.WriteLine("here3");
            string rootDir;
            string tcase_string;
            string recoveredMatFile_index;
            string path;
            string path_file;
            FileStream fs;
            StreamWriter sw;
            switch (et)
            {
                case ExperimentType.Continuous:
                    switch (es)
                    {
                        case ExperimentScenario.Missing:
                            missingBlocks[0].Item3 = tcase;
                            missingBlocks[0].Item2 = rows - tcase;
                            break;
                        
                        case ExperimentScenario.Length:
                            missingBlocks[0].Item2 = tcase - missingBlocks[0].Item3;
                            break;

                        case ExperimentScenario.Columns:
                            break;//nothing
                        
                        case ExperimentScenario.Fullrow:
                            missingBlocks = Enumerable.Range(0, columns).Select(x => (x, rows - tcase, tcase)).ToArray();
                            break;
                        
                        default:
                            throw new ArgumentException("Unrecognized experiment scenario or an incompatible combination with experiment type");
                    }
                    break;
                    
                case ExperimentType.Recovery:
                    switch (es)
                    {
                        case ExperimentScenario.Missing:
                            missingBlocks[0].Item3 = tcase;
                            rootDir = DataWorks.FolderPlotsRemote + $"{es.ToLongString()}/{code2}/";
                            Directory.CreateDirectory(rootDir + "index/");
                            tcase_string = tcase.ToString();
                            recoveredMatFile_index = rootDir + "index/" + $"index"+ tcase_string + ".txt";
                            if (File.Exists(recoveredMatFile_index)) File.Delete(recoveredMatFile_index);
                            path = rootDir + "index/";
                            path_file = path + "index" + tcase_string + ".txt";
                            
                            fs =  new FileStream(path_file,FileMode.Create, FileAccess.Write, FileShare.Write);
                            sw=new StreamWriter(path_file, true);
                            //sw.Write("les");
                            for(int p=0;p<missingBlocks.Length;p++)
                            {
                             for(int i=0;i<missingBlocks[0].Item3;i++)
                             {
                              sw.Write("(" + missingBlocks[p].Item1 + ", " + (missingBlocks[p].Item2+i) +")");
                              sw.Write("\n");   
                             }                        
                            }
                            fs.Close();
                            sw.Close();
                            
                            break;
                        
                        case ExperimentScenario.Length:
                            break;//nothing

                        case ExperimentScenario.Columns:
                            break;//nothing
                        
                        case ExperimentScenario.MultiColumnDisjoint:
                            missingBlocks = Enumerable.Range(0, columns).Select(col => (col, col * MulticolBlockSize, MulticolBlockSize)).Take(tcase).ToArray();
                            rootDir = DataWorks.FolderPlotsRemote + $"{es.ToLongString()}/{code2}/";
                            Directory.CreateDirectory(rootDir + "index/");
                            recoveredMatFile_index = rootDir + "index/" + $"index.txt";
                            if (File.Exists(recoveredMatFile_index)) File.Delete(recoveredMatFile_index);
                            path = rootDir + "index/";
                             path_file = path + "index" + ".txt";
                            
                            fs =  new FileStream(path_file,FileMode.Create, FileAccess.Write, FileShare.Write);
                            sw=new StreamWriter(path_file, true);
                            //sw.Write("les");
                            for(int p=0;p<missingBlocks.Length;p++)
                            {
                             for(int i=0;i<MulticolBlockSize;i++)
                             {
                              sw.Write("(" + missingBlocks[p].Item1 + ", " + (missingBlocks[p].Item2+i) +")");
                              sw.Write("\n");   
                             }                        
                            }
                            fs.Close();
                            sw.Close();
                            break;
                        
                        case ExperimentScenario.MulticolumnOverlap:
                            missingBlocks = Enumerable.Range(0, columns).Select(col => (col, col * MulticolBlockSize, col == columns - 1 ? MulticolBlockSize : MulticolBlockSize * 2)).Take(tcase).ToArray();
                            return;
                        
                        case ExperimentScenario.Fullrow:
                            missingBlocks = Enumerable.Range(0, columns).Select(col => (col, rows / 20, tcase)).ToArray();
                            break;
                        
                        case ExperimentScenario.Fullcolumn:
                            missingBlocks = Enumerable.Range(0, tcase).Select(x => (columns - x - 1, 0, rows)).ToArray();
                            break;
                        
                        case ExperimentScenario.MissingSubMatrix:
                            Console.WriteLine("here3");
                            rootDir = DataWorks.FolderPlotsRemote + $"{es.ToLongString()}/{code}/";
                            Console.WriteLine(rootDir);
                            Console.WriteLine(code2);
                            Console.WriteLine("what");
                            int mcar_block = rows / 10;
                            
                            Console.WriteLine(mcar_block);
                            if(mcar_block == 0)
                            {
                            mcar_block = 1;
                            }
                            const int mcar_percentage = 20;
                            List<(int, int, int)> missing2 = new List<(int, int, int)>();
                            Random r = new Random();
                            //const int mcar_block_real = 4;
                            int activeColumns = (columns * tcase) / 100; // 10 to 100%
                            if(activeColumns == 0)
                            {
                            activeColumns = 1;
                            }

                            List<(int, int)> missing = new List<(int, int)>();
                            
                            Dictionary<int, List<int>> columnIdx = new Dictionary<int, List<int>>();

                            for (int i = 0; i < activeColumns; i++)
                            {
                                columnIdx.Add(i, Enumerable.Range(0, (rows /mcar_block)-1).ToList());
                                
                            }
                            int elegxos = rows/mcar_block;
                            int elpida = (rows * activeColumns * mcar_percentage) / (rows * mcar_block);
                            
                            int whatever = (rows * activeColumns * mcar_percentage) / 100; 
                            Console.WriteLine("here4");
                            Console.WriteLine(whatever);
                            Console.WriteLine(whatever/mcar_block);
                            Console.WriteLine("done");
                            for (int i = 0; i < (whatever) / (mcar_block);  i++) // (rows * activeColumns) / ( mcar_percentage * mcar_block);;;; 100 for percentage adj (rows * activeColumns * mcar_percentage) / (rows * mcar_block)
                            {
                                
                                int po=1;
                                int col_index=0;
                                Console.WriteLine(columnIdx.Count);
                                Console.WriteLine(tcase);
                                Console.WriteLine("palkia");
                                col_index = r.Next(0, columnIdx.Count); //col_index = r.Next(0, columnIdx.Count-1); 
                                int col = columnIdx.Keys.ElementAt(col_index);
                                int row = r.Next(0, columnIdx[col].Count);
                                row = columnIdx[col][row];
                                
                                for (int j = 0; j < mcar_block; j++)//<block_size
                                {
                                    missing.Add((col, mcar_block * row + j));
                                }
                                
                                columnIdx[col].Remove(row);
                                
                                if (columnIdx[col].Count == 1) //  allagi edo gia to 24_120 apo ==3 (elegxos -1)
                                {
                                
                                columnIdx.Remove(col);
                                
                                }
                                
                            }
                           
                            if(tcase==100)
                            {
                            rootDir = DataWorks.FolderPlotsRemote + $"{es.ToLongString()}/{code2}/";
                            
                            
                            Directory.CreateDirectory(rootDir + "index/");
                            recoveredMatFile_index = rootDir + "index/" + $"index.txt";
                            if (File.Exists(recoveredMatFile_index)) File.Delete(recoveredMatFile_index);
                            path = rootDir + "index/";
                            path_file = path + "index" + ".txt";
                            
                            fs =  new FileStream(path_file,FileMode.Create, FileAccess.Write, FileShare.Write);
                            sw = new StreamWriter(path_file, true);
                            
                            
                            for(int p=0;p<missing.Count;p++)
                            {
                                sw.Write(missing[p]);
                                sw.Write("\n");                           
                            }
                            fs.Close();
                            sw.Close();
                            }
                            
                            missing = missing.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();
                            
                            int currentCol = -1;
                            int blockStart = -1;
                            int lastIdx = -1;
                            
                            for (int i = 0; i < missing.Count; i++)
                            {
                                (int col, int row) = missing[i];
                                
                                if (currentCol == col) //same col
                                {
                                    if (lastIdx == -1) // start of new block
                                    {
                                        lastIdx = row;
                                        blockStart = row;
                                    }
                                    else if (lastIdx != row - 1) // jump to the next block
                                    {
                                        missing2.Add((col, blockStart, lastIdx - blockStart + 1));
                                        blockStart = lastIdx = row;
                                    }
                                    else
                                    {
                                        lastIdx = row;
                                    }
                                }
                                else
                                {
                                    if (blockStart >= 0)
                                    {
                                        missing2.Add((currentCol, blockStart, lastIdx - blockStart + 1));
                                        
                                    }

                                    blockStart = lastIdx = row;
                                    currentCol = col;
                                }
                            }
                            
                            if (blockStart >= 0)
                            {
                                missing2.Add((currentCol, blockStart, lastIdx - blockStart + 1));
                               
                            }

                            missingBlocks = missing2.ToArray();
                            break;
                        
                        default:
                            throw new ArgumentException("Unrecognized experiment scenario");
                    }
                    break;
                
                case ExperimentType.Streaming:
                    switch (es)
                    {
                        default:
                            throw new ArgumentException("Unrecognized experiment scenario");
                    }
                
                default:
                    throw new ArgumentException("Unrecognized experiment type");
            }
        }

        private static int GetGnuPlotStartingNumber(ExperimentType et, ExperimentScenario es, int nlimit, int tcase)
        {
            if ((et == ExperimentType.Streaming || et == ExperimentType.Continuous) && es == ExperimentScenario.Length)
                return nlimit - tcase;
            
            return 0;
        }

        private static DataDescription PrepareDataDescription(ExperimentType et, ExperimentScenario es,
            string code, int rows, int cols, int tcase, (int, int, int)[] missingBlocks)
        {
            int n = es == ExperimentScenario.Length
                ? tcase
                : rows;
            
            int m = es == ExperimentScenario.Columns
                ? tcase
                : cols;
            
            return new DataDescription(n, m, missingBlocks, code);
        }

        private static ((int, int), (int, int)) GetDataRanges(ExperimentType et, ExperimentScenario es,
            int nlimit, int cols, int tcase)
        {
            (int rFrom, int rTo) = (0, nlimit);
            (int cFrom, int cTo) = (0, cols);

            switch (es)
            {
                // columns always start from 0, limited by datasize unless it's a column test
                case ExperimentScenario.Columns:
                    cTo = tcase;
                    break;
                
                // varlengths, it's 0...tcase
                case ExperimentScenario.Length:
                    rTo = tcase;
                    break;
            }

            return ((rFrom, rTo), (cFrom, cTo));
        }

        #endregion
        
        /// <summary>
        /// Runs a precision experiment of type <paramref name="et"/> with scenario <paramref name="es"/> on a dataset <paramref name="code"/>.
        /// </summary>
        /// <param name="et">Experiment type</param>
        /// <param name="es">Experiment scenario</param>
        /// <param name="code">Dataset codename</param>
        /// <param name="nlimit">Maximum length from the dataset</param>
        /// <exception cref="ArgumentException">Throws an exception if incompatible type/scenario are provided or a code isn't found.</exception>
        public static void PrecisionTest(
            ExperimentType et, ExperimentScenario es,
            string code)
        {
            if (et == ExperimentType.Streaming)
            {
                throw new ArgumentException("ExperimentType.Streaming is unsupported for precision test runs");
            }
            if (!File.Exists($"{DataWorks.FolderData}{code}/{code}_normal.txt"))
            {
                throw new ArgumentException("Invalid code is supplied, file not found in a expected location: " + $"{code}/{code}_normal.txt");
            }
            
            List<int> cdk = null;
            if (es == ExperimentScenario.Fullrow && ((CentroidDecompositionRecoveryAlgorithm)AlgoPack.CdRec).KList.Count > 1)
            {
                cdk = ((CentroidDecompositionRecoveryAlgorithm)AlgoPack.CdRec).KList;
                ((CentroidDecompositionRecoveryAlgorithm)AlgoPack.CdRec).KList = new List<int>(new[] { 2, 1 });
            }
            
            int nlimit = DataWorks.CountMatrixRows($"{code}/{code}_normal.txt");
            int dataSetColumns = DataWorks.CountMatrixColumns($"{code}/{code}_normal.txt");
            
            IEnumerable<Algorithm> algorithms =
                es.IsSingleColumn()
                    ? AlgoPack.ListAlgorithms
                    : AlgoPack.ListAlgorithmsMulticolumn;

            if (nlimit < 1000)
            {
                algorithms = algorithms.Where(alg => alg.AlgCode != "tkcm" && alg.AlgCode != "spirit").ToArray();
            }

            if (algorithms.Count() == 0)
            {
                Utils.DelayedWarnings.Enqueue($"Scenario {es.ToLongString()} (precision) was launched with no compatible algorithms and will not be performed.");
                return;
            }
            
            //varlen only
            ulong token =
                (code + et.ToLongString() + es.ToLongString()).ToCharArray()
                .Select((x, i) => (UInt64) (i * Math.Abs(Math.PI * x)))
                .Aggregate(0UL, (i, arg2) => i ^ arg2);
            
            // forward definitons
            const Experiment ex = Experiment.Precision;
            (ValueTuple<int, int, int>[] missingBlocks, int[] lengths) = GetExperimentSetup(et, es, nlimit, dataSetColumns, code);

            //
            // create necessary folder structure
            //

            foreach (int tcase in lengths)
            {
                if (!Directory.Exists(DataWorks.FolderResultsPlots + tcase))
                {
                    Directory.CreateDirectory(DataWorks.FolderResultsPlots + tcase);
                    Directory.CreateDirectory(DataWorks.FolderResultsPlots + tcase + "/raw");
                }
            }
            
            //
            // test phase
            //

            if (et == ExperimentType.Continuous && es == ExperimentScenario.Length)
            {
                string dataSource = $"{code}/{code}_normal.txt";
                
                foreach (int tcase in lengths)
                {
                    string adjustedDataSource = $"_.temp/{token}_{code}_{tcase}.txt";

                    if (File.Exists($"{DataWorks.FolderData}" + adjustedDataSource)) File.Delete($"{DataWorks.FolderData}" + adjustedDataSource);
                    DataWorks.DataRange(dataSource, adjustedDataSource, nlimit - tcase, tcase);
                }
            }

            //do it
            foreach (Algorithm alg in algorithms)
            {
                foreach (int tcase in lengths)
                {
                    string dataSource = $"{code}/{code}_normal.txt";

                    if (et == ExperimentType.Continuous && es == ExperimentScenario.Length)
                    {
                        string adjustedDataSource = $"_.temp/{token}_{code}_{tcase}.txt";
                        dataSource = adjustedDataSource;
                    }
                    int codef =0;
                    Console.WriteLine("here");
                    UpdateMissingBlocks(et, es, nlimit, tcase, ref missingBlocks, dataSetColumns,codef,code);

                    var (rowRange, columnRange) = GetDataRanges(et, es, nlimit, dataSetColumns, tcase);
                    var data = PrepareDataDescription(et, es, code, nlimit, dataSetColumns, tcase, missingBlocks);
                    
                    alg.GenerateData(dataSource, code, tcase, missingBlocks, rowRange, columnRange);
                    alg.RunExperiment(ex, et, es, data, tcase);
                }
                
                if (alg.AlgCode == "mvexport")
                {
                    //just plain return, nothing more to do here
                    // if this condition is true this is the only algo in the set
                    // and it will leave no trash apart from the output
                    return;
                }

                alg.CollectResults(ex, DataWorks.FolderResults,
                    lengths.Select(x => alg.EnumerateOutputFiles(x)).Flatten().ToArray());
            }

            //
            // add GNUPLOT
            //

            foreach (int tcase in lengths)
            {
                string[] allFiles = algorithms.Select(
                    alg => alg.EnumerateOutputFiles(tcase)
                ).Flatten().ToArray();
                    
                DataWorks.AddGnuPlotNumeration(false, tcase + "/",
                    GetGnuPlotStartingNumber(et, es, nlimit, tcase),
                    allFiles);
            }

            //
            // MSE/Correlation test
            //

            foreach (int tcase in lengths)
            {
                int code2 =1;
                UpdateMissingBlocks(et, es, nlimit, tcase, ref missingBlocks, dataSetColumns,code2,code);

                string referenceMatrix = $"../{DataWorks.FolderData}{code}/{code}_normal.txt";
                
                if (et == ExperimentType.Continuous && es == ExperimentScenario.Length)
                {
                    referenceMatrix = $"../{DataWorks.FolderData}_.temp/{token}_{code}_{tcase}.txt";
                }
                
                string[] allFiles = algorithms.Select(
                    alg => alg.EnumerateOutputFiles(tcase)
                ).Flatten().ToArray();

                DataWorks.CalculateMse(
                    missingBlocks, referenceMatrix, $"missingMat/missingMat{tcase}.txt",
                    allFiles
                );
            }

            //
            // GNUPLOT plt files
            //
            foreach (int tcase in lengths)
            {
                int code4=2;
                UpdateMissingBlocks(et, es, nlimit, tcase, ref missingBlocks, dataSetColumns,code4,code);

                int offset = (et == ExperimentType.Continuous && es == ExperimentScenario.Length) ? nlimit - tcase : 0;
                DataWorks.GeneratePrecisionGnuPlot(algorithms, code, nlimit, tcase, missingBlocks, offset);
            }

            string referenceData = $"{DataWorks.FolderResults}{code}_normal.txt";
            if (File.Exists(referenceData)) File.Delete(referenceData);
            
            File.Copy($"{DataWorks.FolderData}{code}/{code}_normal.txt", referenceData);
            
            DataWorks.AddGnuPlotNumeration(false, "", 0, $"{code}_normal.txt"); // now the file resides in results/plots/file.txt
            
            //
            // Move results to proper folders
            //
            
            string rootDir = DataWorks.FolderPlotsRemote + $"{es.ToLongString()}/{code}/";

            if (Directory.Exists(rootDir))
            {
                string tempf;
                // clean up ONLY precision results
                if (Directory.Exists(tempf = rootDir + "error/")) Directory.Delete(tempf, true);
                if (Directory.Exists(tempf = rootDir + "recovery/")) Directory.Delete(tempf, true);
                //if (Directory.Exists(tempf = rootDir + "index/")) Directory.Delete(tempf, true);
                
            }
            else
            {
                Directory.CreateDirectory(rootDir);
            }

            {
                Directory.CreateDirectory(rootDir + "error/");
                Directory.CreateDirectory(rootDir + "recovery/");
                
                Directory.CreateDirectory(rootDir + "error/mae/");
                Directory.CreateDirectory(rootDir + "error/mse/");
                Directory.CreateDirectory(rootDir + "error/rmse/");
                Directory.CreateDirectory(rootDir + "error/mape/");
                Directory.CreateDirectory(rootDir + "error/plots/");

                Directory.CreateDirectory(rootDir + "recovery/plots/");
                Directory.CreateDirectory(rootDir + "recovery/values/");
                Directory.CreateDirectory(rootDir + "recovery/values/recovered_matrices/");
                

                // part of path is shared with rt
                Directory.CreateDirectory(rootDir + "scripts/precision/");
            }
            
            Console.WriteLine("Copying over results");

            // case'd things
            foreach (int tcase in lengths)
            {
                // gnuplots
                string dataTCaseFolder = rootDir + "recovery/values/" + tcase + "/";
                if (Directory.Exists(dataTCaseFolder)) Directory.Delete(dataTCaseFolder, true);
                
                Directory.Move(
                    DataWorks.FolderResultsPlots + tcase + "/",
                    dataTCaseFolder);
                
                // plotfiles
                string pltFile = rootDir + $"scripts/precision/{code}_m{tcase}.plt";
                if (File.Exists(pltFile)) File.Delete(pltFile);
                    
                File.Move(
                    DataWorks.FolderResults + "plotfiles/out/" + $"{code}_m{tcase}.plt",
                    pltFile);
                
                // recovered matrices
                string recoveredMatFile = rootDir + "recovery/values/recovered_matrices/" + $"recoveredMat{tcase}.txt";
                if (File.Exists(recoveredMatFile)) File.Delete(recoveredMatFile);
                
                File.Move(
                    DataWorks.FolderResults + "missingMat/" + $"missingMat{tcase}.txt",
                    recoveredMatFile);
            }
            // independent things
            int start = lengths.First(), end = lengths.Last(), tick = lengths.Skip(1).First() - start;
            
            // mse
            DataWorks.GenerateMseGnuPlot(algorithms, code, start, end, tick, es);

            string mseFile = rootDir + $"scripts/precision/{code}_mse.plt";
            if (File.Exists(mseFile)) File.Delete(mseFile);
            
            File.Copy($"{DataWorks.FolderResults}plotfiles/out/{code}_mse.plt", mseFile);
            
            // Rscript
            Utils.FileFindAndReplace(DataWorks.FolderResults + "plotfiles/template_err.r",
                $"{rootDir}scripts/precision/error_calculation.r",
                ("{start}", start.ToString()),
                ("{end}", end.ToString()),
                ("{tick}", tick.ToString()),
                ("{allAlgos}", algorithms
                    .Select(a => a.EnumerateSubAlgorithms())
                    .Flatten()
                    .Select(sa => $"\"{sa.Code}\"")
                    .StringJoin(","))
                );
            
            // plotall
            Utils.FileFindAndReplace(DataWorks.FolderResults + "plotfiles/template_plotall.py",
                $"{rootDir}scripts/precision/plotall.py",
                ("{code}", code),
                ("{start}", start.ToString()),
                ("{end}", end.ToString()),
                ("{tick}", tick.ToString()),
                ("{vis}", DataWorks.DisableVisualization ? "#" : "")
            );

            // reference plot
            if (dataSetColumns > 4)
            {
                // copy 6-column reference
                Utils.FileFindAndReplace(DataWorks.FolderResults + "plotfiles/reference_plot_6.plt",
                    $"{rootDir}scripts/precision/reference_plot.plt",
                    ("{nlimit}", Math.Min(nlimit, 2500).ToString())
                );
            }
            else
            {
                // copy 4-column reference
                Utils.FileFindAndReplace(DataWorks.FolderResults + "plotfiles/reference_plot_4.plt",
                    $"{rootDir}scripts/precision/reference_plot.plt",
                    ("{nlimit}", Math.Min(nlimit, 2500).ToString())
                );
            }
            
            if (File.Exists($"{rootDir}recovery/values/reference.txt")) File.Delete($"{rootDir}recovery/values/reference.txt");
            File.Move($"{DataWorks.FolderResultsPlots}{code}_normal.txt", $"{rootDir}recovery/values/reference.txt");
            
            Console.WriteLine("Plotting results");
            
            Utils.RunSimpleVoidProcess("python", rootDir, "scripts/precision/plotall.py");
            
            Console.WriteLine($"Sequence {ex.ToLongString()} / {et.ToLongString()} / {es.ToLongString()} for {code} completed");
            
            //
            // cleanup
            //
            Console.WriteLine("Starting cleanup...");
            if (cdk != null)
            {
                ((CentroidDecompositionRecoveryAlgorithm)AlgoPack.CdRec).KList = cdk;
            }
            AlgoPack.PurgeAllIntermediateFiles(); // handles algo's internal in/out fodlers

            Console.WriteLine("Intermediate files cleaned up");

            Directory.EnumerateFiles(DataWorks.FolderResults)
                .Where(x => !Directory.Exists(x)) // hacky-hacky
                .Where(x => !x.StartsWith("scenarios_"))
                .ForEach(File.Delete);
            
            Console.WriteLine("Results folder cleaned up");
            Console.WriteLine("Cleanup finished");
        }

        /*///////////////////////////////////////////////////////////*/
        
        
        /// <summary>
        /// Runs a runtime experiment of type <paramref name="et"/> with scenario <paramref name="es"/> on a dataset <paramref name="code"/>.
        /// </summary>
        /// <param name="et">Experiment type</param>
        /// <param name="es">Experiment scenario</param>
        /// <param name="code">Dataset codename</param>
        /// <param name="nlimit">Maximum length from the dataset</param>
        /// <exception cref="ArgumentException">Throws an exception if incompatible type/scenario are provided or a code isn't found.</exception>
        public static void RuntimeTest(
            ExperimentType et, ExperimentScenario es,
            string code)
        {
            IEnumerable<Algorithm> algorithms =
                et == ExperimentType.Streaming
                    ? throw new Exception("streaming is unsupported")
                    : (
                        es.IsSingleColumn()
                            ? AlgoPack.ListAlgorithms
                            : AlgoPack.ListAlgorithmsMulticolumn
                    );
            
            if (!File.Exists($"{DataWorks.FolderData}{code}/{code}_normal.txt"))
            {
                throw new ArgumentException("Invalid code is supplied, file not found in a expected location: " + $"{code}/{code}_normal.txt");
            }
            
            int nlimit = DataWorks.CountMatrixRows($"{code}/{code}_normal.txt");
            int dataSetColumns = DataWorks.CountMatrixColumns($"{code}/{code}_normal.txt");

            if (nlimit < 1000)
            {
                algorithms = algorithms.Where(alg => alg.AlgCode != "tkcm" && alg.AlgCode != "spirit").ToArray();
            }

            if (algorithms.Count() == 0)
            {
                Utils.DelayedWarnings.Enqueue($"Scenario {es.ToLongString()} (runtime) was launched with no compatible algorithms and will not be performed.");
                return;
            }
            
            //varlen only
            ulong token =
                (code + et.ToLongString() + es.ToLongString()).ToCharArray()
                .Select((x, i) => (UInt64) (i * Math.Abs(Math.PI * x)))
                .Aggregate(0UL, (i, arg2) => i ^ arg2);
            
            // forward definitons
            const Experiment ex = Experiment.Runtime;
            (ValueTuple<int, int, int>[] missingBlocks, int[] lengths) = GetExperimentSetup(et, es, nlimit, dataSetColumns, code);

            //
            // create necessary folder structure
            //

            foreach (int tcase in lengths)
            {
                if (!Directory.Exists(DataWorks.FolderResultsPlots + tcase))
                {
                    Directory.CreateDirectory(DataWorks.FolderResultsPlots + tcase);
                    Directory.CreateDirectory(DataWorks.FolderResultsPlots + tcase + "/raw");
                }
            }
            
            //
            // test phase
            //

            if (et == ExperimentType.Continuous && es == ExperimentScenario.Length)
            {
                string dataSource = $"{code}/{code}_normal.txt";
                
                foreach (int tcase in lengths)
                {
                    string adjustedDataSource = $"_.temp/{token}_{code}_{tcase}.txt";

                    if (File.Exists($"{DataWorks.FolderData}" + adjustedDataSource)) File.Delete($"{DataWorks.FolderData}" + adjustedDataSource);
                    DataWorks.DataRange(dataSource, adjustedDataSource, nlimit - tcase, tcase);
                }
            }

            //do it
            foreach (Algorithm alg in algorithms)
            {
                foreach (int tcase in lengths)
                {
                    string dataSource = $"{code}/{code}_normal.txt";

                    if (et == ExperimentType.Continuous && es == ExperimentScenario.Length)
                    {
                        string adjustedDataSource = $"_.temp/{token}_{code}_{tcase}.txt";
                        dataSource = adjustedDataSource;
                    }
                    int code3 =3;
                    UpdateMissingBlocks(et, es, nlimit, tcase, ref missingBlocks, dataSetColumns,code3,code);

                    var (rowRange, columnRange) = GetDataRanges(et, es, nlimit, dataSetColumns, tcase);
                    var data = PrepareDataDescription(et, es, code, nlimit, dataSetColumns, tcase, missingBlocks);
                    
                    alg.GenerateData(dataSource, code, tcase, missingBlocks, rowRange, columnRange);
                    alg.RunExperiment(ex, et, es, data, tcase);
                }

                alg.CollectResults(ex, DataWorks.FolderResults,
                    lengths.Select(x => alg.EnumerateOutputFiles(x)).Flatten().ToArray());
            }
            
            //
            // create outputs
            //

            string rootDir = DataWorks.FolderPlotsRemote + $"{es.ToLongString()}/{code}/";
            
            if (Directory.Exists(rootDir))
            {
                string tempf;
                // clean up ONLY precision results
                if (Directory.Exists(tempf = rootDir + "runtime/")) Directory.Delete(tempf, true);
                if (Directory.Exists(tempf = rootDir + "scripts/rutnime/")) Directory.Delete(tempf, true);
            }
            else
            {
                Directory.CreateDirectory(rootDir);
            }

            {
                Directory.CreateDirectory(rootDir + "runtime/");
                Directory.CreateDirectory(rootDir + "runtime/values/");
                Directory.CreateDirectory(rootDir + "runtime/plots/");
                
                // part of path is shared with prec
                Directory.CreateDirectory(rootDir + "scripts/runtime/");
            }

            Console.WriteLine("Copying over results");

            //
            // add GNUPLOT
            //
            
            DataWorks.CollectRuntimeResults(lengths, algorithms, rootDir + "runtime/values/");

            //
            // GNUPLOT plt files
            //
            int start = lengths.First(), end = lengths.Last(), tick = lengths.Skip(1).First() - start;

            DataWorks.GenerateRuntimeGnuPlot(algorithms, code, start, end, tick, et, es);
            
            string plotFileExt = rootDir + $"scripts/runtime/{code}_rt.plt";

            if (File.Exists(plotFileExt)) File.Delete(plotFileExt);
            
            File.Move($"{DataWorks.FolderResults}plotfiles/out/{code}_rt.plt", plotFileExt);
            
            Console.WriteLine("Plotting results");
            Utils.RunSimpleVoidProcess("gnuplot", rootDir, $"scripts/runtime/{code}_rt.plt");

            //
            // cleanup
            //
            Console.WriteLine("Starting cleanup...");
            AlgoPack.PurgeAllIntermediateFiles(); // handles algo's internal in/out fodlers

            Console.WriteLine("Intermediate files cleaned up");
            
            Directory.EnumerateFiles(DataWorks.FolderResults)
                .Where(x => !Directory.Exists(x))
                .ForEach(File.Delete);
            
            Console.WriteLine("Gnuplot folder cleaned up");
            Console.WriteLine("Cleanup finished");
            
            Console.WriteLine($"Sequence {ex.ToLongString()} / {et.ToLongString()} / {es.ToLongString()} for {code} completed");
        }
        
        /// <summary>
        /// Plots the results of a runtime experiment of type <paramref name="et"/> with scenario <paramref name="es"/> on a dataset <paramref name="code"/>.
        /// Overwrites old templates, only to be executed on top of an experiment with the same parameters. Doesn't re-run the experiment.
        /// </summary>
        /// <param name="et">Experiment type</param>
        /// <param name="es">Experiment scenario</param>
        /// <param name="code">Dataset codename</param>
        /// <param name="nlimit">Maximum length from the dataset</param>
        /// <exception cref="ArgumentException">Throws an exception if incompatible type/scenario are provided or a code isn't found.</exception>
        /// <exception cref="InvalidOperationException">Throws an exception if the folder for this specific experiment set up doesn't exist.</exception>
        public static void RuntimeTestReplot(
            ExperimentType et, ExperimentScenario es,
            string code)
        {
            if (!File.Exists($"{DataWorks.FolderData}{code}/{code}_normal.txt"))
            {
                throw new ArgumentException("Invalid code is supplied, file not found in a expected location: " + $"{code}/{code}_normal.txt");
            }
            
            IEnumerable<Algorithm> algorithms =
                et == ExperimentType.Streaming
                    ? throw new Exception("streaming is unsupported")
                    : (
                        es == ExperimentScenario.MultiColumnDisjoint
                            ? AlgoPack.ListAlgorithmsMulticolumn
                            : AlgoPack.ListAlgorithms
                    );
            
            int nlimit = DataWorks.CountMatrixRows($"{code}/{code}_normal.txt");
            int dataSetColumns = DataWorks.CountMatrixColumns($"{code}/{code}_normal.txt");
            
            // forward definitons
            const Experiment ex = Experiment.Runtime;
            (_, int[] lengths) = GetExperimentSetup(et, es, nlimit, dataSetColumns, code);

            //
            // create outputs
            //

            string rootDir = DataWorks.FolderPlotsRemote +
                             $"{ex.ToLongString()}/{et.ToLongString()}/{es.ToLongString()}/{code}/";
            
            if (!Directory.Exists(rootDir))
            {
                throw new InvalidOperationException();
            }

            //
            // GNUPLOT plt files
            //
            int start = lengths.First(), end = lengths.Last(), tick = lengths.Skip(1).First() - start;

            DataWorks.GenerateRuntimeGnuPlot(algorithms, code, start, end, tick, et, es);
            
            string plotFileExt = rootDir + $"{code}_rt.plt";

            if (File.Exists(plotFileExt)) File.Delete(plotFileExt);
            
            File.Move($"{DataWorks.FolderResults}plotfiles/out/{code}_rt.plt", plotFileExt);
            
            Console.WriteLine("Plotting results");
            Utils.RunSimpleVoidProcess("gnuplot", rootDir, $"{code}_rt.plt");

            //
            // cleanup
            //
            
            Console.WriteLine($"Sequence {ex.ToLongString()} / {et.ToLongString()} / {es.ToLongString()} [REPLOT] for {code} completed");
        }
    }
}
