using APP_CONFIG.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using TRANSACTION_SEARCH.Analyzers;

namespace FILE_SORT.Helpers
{
    public static class TransactionSearch
    {
        private const string deviceUIListenerSignature = "DeviceUI Listener";
        private const string flyweightWorkerSignature = "Flyweight Worker";
        private const string paymentSegmentKey = "Payment sale started, RequestID:";
        private const string guidFinderPattern = @"(RequestID:)\s*(\ )([a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12})";
        private static List<string> guidPaymentList;

        public static bool SearchTransaction(List<FileGroup> transactionToSearch)
        {
            bool result = false;

            try
            {
                string exeLocation = Assembly.GetExecutingAssembly().GetName().CodeBase;
                UriBuilder uri = new UriBuilder(exeLocation);
                string targetDir = Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path)), "in");
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                foreach (FileGroup transaction in transactionToSearch)
                {
                    string fileInPath = Path.Combine(targetDir, transaction.Input);

                    if (File.Exists(fileInPath))
                    {
                        string headerString = $"========== PROCESSING FILE {transaction.Input} ==========";
                        Console.WriteLine("-".PadRight(headerString.Length, '-'));
                        Console.WriteLine(headerString);
                        Console.WriteLine("-".PadRight(headerString.Length, '-'));
                        Console.WriteLine();

                        string[] logFile = File.ReadAllLines(fileInPath);
                        List<string> logList = new List<string>(logFile);

                        // setup guidPaymentList
                        FilterPaymentSegments(logList);

                        if (guidPaymentList.Count > 0)
                        {
                            int sequenceIndex = 0;
                            foreach (string paymentGuid in guidPaymentList)
                            {
                                List<string> transactionLog = logList.FindAll(x => x.Contains(paymentGuid));

                                if (transaction.FilterOut.FlightWeightWorker)
                                {
                                    transactionLog = transactionLog.FindAll(x => !x.Contains(flyweightWorkerSignature));
                                }

                                if (transaction.FilterOut.DeviceUIListener)
                                {
                                    transactionLog = transactionLog.FindAll(x => !x.Contains(deviceUIListenerSignature));
                                }

                                string fileOutName = string.Format("{0:D3}_{1}", ++sequenceIndex, paymentGuid.Trim(new Char[] { '[', ']' }) + ".txt");
                                string fileOutPath = Path.Combine(targetDir, fileOutName);
                                File.WriteAllLines(fileOutPath, transactionLog);

                                // load file to NotePad++
                                //ProcessHelper.LoadFileToEditor(fileOutPath);

                                // Analyze Payload
                                MasterAnalyzer.Analyze(paymentGuid, transaction.Request, transactionLog);

                                result = true;
                            }
                        }
                        else
                        {
                            Console.WriteLine("NO PAYMENT REQUESTS FOUND.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"FILE [{fileInPath}] - not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in fileReader={ex.Message}");
            }
            return result;
        }

        public static bool SearchTransaction(List<Tuple<string, string, string>> transactionToSearch)
        {
            bool result = false;

            try
            {
                string exeLocation = Assembly.GetExecutingAssembly().GetName().CodeBase;
                UriBuilder uri = new UriBuilder(exeLocation);
                string targetDir = Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path)), "in");
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                foreach ((string fileIn, string guid, string request) in transactionToSearch)
                {
                    string fileInPath = Path.Combine(targetDir, fileIn);

                    if (File.Exists(fileInPath))
                    {
                        string headerString = $"========== PROCESSING FILE {fileIn} ==========";
                        Console.WriteLine("-".PadRight(headerString.Length, '-'));
                        Console.WriteLine(headerString);
                        Console.WriteLine("-".PadRight(headerString.Length, '-'));
                        Console.WriteLine();

                        string[] logFile = File.ReadAllLines(fileInPath);
                        List<string> logList = new List<string>(logFile);

                        // setup guidPaymentList
                        FilterPaymentSegments(logList);

                        if (guidPaymentList.Count > 0)
                        {
                            foreach (string paymentGuid in guidPaymentList)
                            {
                                List<string> transactionLog = logList.FindAll(x => x.Contains(paymentGuid));

                                string fileOutPath = Path.Combine(targetDir, paymentGuid.Trim(new Char[] { '[', ']' }) + ".txt");
                                File.WriteAllLines(fileOutPath, transactionLog);

                                // load file to NotePad++
                                //ProcessHelper.LoadFileToEditor(fileOutPath);

                                // Analyze Payload
                                MasterAnalyzer.Analyze(paymentGuid, request, transactionLog);

                                result = true;
                            }
                        }
                        else
                        {
                            Console.WriteLine("NO PAYMENT REQUESTS FOUND.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"FILE [{fileInPath}] - not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in fileReader={ex.Message}");
            }
            return result;
        }

        private static void FilterPaymentSegments(List<string> filePayload)
        {
            guidPaymentList = new List<string>();

            List<string> transactionLog = filePayload.FindAll(x => x.Contains(paymentSegmentKey));

            foreach (string transaction in transactionLog)
            {
                Regex rg = new Regex(guidFinderPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                MatchCollection match = rg.Matches(transaction);
                if (match.Count > 0 && match[0].Groups.Count == 4)
                {
                    guidPaymentList.Add(match[0].Groups[3].Value);
                }
            }
        }
    }
}
