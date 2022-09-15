using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TRANSACTION_SEARCH.Analyzers;
using TRANSACTION_SEARCH.Helpers;

namespace FILE_SORT.Helpers
{
    public static class TransactionSearch
    {
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
                    string fileOutPath = Path.Combine(targetDir, guid.Trim(new Char[] { '[', ']' }) + ".txt");

                    if (File.Exists(fileInPath))
                    {
                        string[] logFile = File.ReadAllLines(fileInPath);
                        List<string> logList = new List<string>(logFile);

                        List<string> transactionLog = logList.FindAll(x => x.Contains(guid));
                        File.WriteAllLines(fileOutPath, transactionLog);

                        // load file to NotePad++
                        //ProcessHelper.LoadFileToEditor(fileOutPath);

                        // Analyze Payload
                        MasterAnalyzer.Analyze(guid, request, transactionLog);

                        result = true;
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
    }
}
