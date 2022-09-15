using System;
using System.Collections.Generic;
using TRANSACTION_SEARCH.Analyzers.Models;

namespace TRANSACTION_SEARCH.Analyzers
{
    public static class ServicerCore
    {
        private const int paddingSpaces = 10;
        private const string servicerCoreText = @"[IPA5.Servicer.Core]";
        private const string requestTypeIdentifier = "\"PaymentRequest\"";

        private static string transactionGuid;
        private static string transactionRequest;
        private static Dictionary<string, (string, string)> TransactionFlow = new Dictionary<string, (string, string)>();

        private static void LoadTransactionFlow(List<string> payload)
        {
            LogEntrySchema schema = null;
            List<string> transactionLog = payload.FindAll(x => x.Contains(servicerCoreText));
            int count = 0;

            foreach (string item in transactionLog)
            {
                string[] split = item.Split(' ');
                try
                {
                    schema = new LogEntrySchema()
                    {
                        DateStamp = split[(int)LogSchemaIndex.DateStamp],
                        TimeStamp = split[(int)LogSchemaIndex.TimeStamp],
                        UTCOffset = split[(int)LogSchemaIndex.UTCOffset],
                        LogLevel = split[(int)LogSchemaIndex.LogLevel],
                        Assembly = split[(int)LogSchemaIndex.Assembly],
                        Workstation = split[(int)LogSchemaIndex.Workstation],
                        Message = split[(int)LogSchemaIndex.Message]
                    };

                    // Append remainder to Message payload
                    for (int index = (int)LogSchemaIndex.Message + 1; index < split.Length - 2; index++)
                    {
                        schema.Message += $" {split[index]}";
                    }

                    // key must be unique
                    TransactionFlow.Add($"LINE {++count}", (schema.Message, schema.TimeStamp));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DALCore Analyzer exception={ex.Message}");
                }
            }
        }

        public static void Analyze(string guid, string request, List<string> payload)
        {
            transactionGuid = guid;
            transactionRequest = request;
            TransactionFlow = new Dictionary<string, (string, string)>();

            LoadTransactionFlow(payload);

            // Expect pairs for Request Start / Request Complete Pattern
            bool foundOne = false;
            bool hasRequest = false;
            string requestType = string.Empty;
            string startTime = string.Empty;

            Console.WriteLine($"{servicerCoreText} ANALYSIS for {transactionGuid} - REQUEST: `{transactionRequest}`");

            foreach (KeyValuePair<string, (string message, string time)> item in TransactionFlow)
            {
                if (item.Value.message.Contains($"\"DALAction\": \"{transactionRequest}\""))
                {
                    foundOne = true;
                    hasRequest = true;
                    startTime = item.Value.time;
                    List<string> messageDescription = new List<string>(item.Value.message.Split(' '));
                    int index = messageDescription.FindIndex(x => x.Contains(requestTypeIdentifier));
                    if (index > 0)
                    {
                        string[] messageSource = messageDescription[index].Split(':');
                        requestType = $"[{messageSource[0].Trim('"').PadRight(paddingSpaces)} => {transactionRequest}]";
                    }
                }
                else if (hasRequest)
                {
                    hasRequest = false;
                    TimeSpan duration = DateTime.Parse(item.Value.time).Subtract(DateTime.Parse(startTime));
                    Console.WriteLine($"{requestType} :: DURATION: {duration.ToString(@"mm\:ss\.FFF")}\r\n");
                }
            }

            if (!foundOne)
            {
                Console.WriteLine("None found.\r\n");
            }
        }
    }
}
