using System;
using System.Collections.Generic;
using TRANSACTION_SEARCH.Analyzers.Models;

namespace TRANSACTION_SEARCH.Analyzers
{
    public static class DALCore
    {
        private static readonly int paddingSpaces = "GetCreditOrDebit".Length;
        private const string dalCoreText = @"[IPA5.DAL.Core]";
        private const string requestTypeIdentifier = "from:";

        private static string transactionGuid;
        private static Dictionary<string, (string, string)> TransactionFlow;

        private static TimeSpan requestCumulativeTime;
        private static TimeSpan transactionOverallTime;

        private static void LoadTransactionFlow(List<string> payload)
        {
            LogEntrySchema schema = null;
            List<string> transactionLog = payload.FindAll(x => x.Contains(dalCoreText));
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

        public static void Analyze(string guid, List<string> payload)
        {
            transactionGuid = guid;
            TransactionFlow = new Dictionary<string, (string, string)>();

            LoadTransactionFlow(payload);

            // Expect pairs for Request Start / Request Complete Pattern
            bool hasRequest = false;
            bool waitingToComplete = false;
            string requestType = string.Empty;
            string requestStartTime = string.Empty;
            requestCumulativeTime = new TimeSpan();
            string transactionStartTime = string.Empty;
            transactionOverallTime = new TimeSpan();

            Console.WriteLine($"{dalCoreText} ANALYSIS for {transactionGuid}\r\n");

            foreach (KeyValuePair<string, (string message, string time)> item in TransactionFlow)
            {
                if (item.Value.message.Contains("Request of MessageID:"))
                {
                    hasRequest = true;
                    requestStartTime = item.Value.time;
                    if (item.Value.message.Contains("GetPayment"))
                    {
                        transactionStartTime = requestStartTime;
                    }
                    List<string> messageDescription = new List<string>(item.Value.message.Split(' '));
                    int index = messageDescription.FindIndex(x => x.Contains(requestTypeIdentifier));
                    if (index > 0)
                    {
                        string[] messageSource = messageDescription[index].Split(':');
                        requestType = $"[{messageSource[1].PadRight(paddingSpaces)}]";
                    }
                }
                else if (hasRequest)
                {
                    hasRequest = false;
                    waitingToComplete = true;
                    TimeSpan requestDuration = DateTime.Parse(item.Value.time).Subtract(DateTime.Parse(requestStartTime));

                    Console.WriteLine($"{requestType} => RECEIVED__: {requestCumulativeTime.ToString(@"mm\:ss\.FFF").PadRight(9, '0')}");

                    if (item.Value.message.Contains("Request completed."))
                    {
                        waitingToComplete = false;
                        string completedStr = $"COMPLETED_: {requestDuration.ToString(@"mm\:ss\.FFF").PadRight(9, '0')}";
                        Console.WriteLine(completedStr.PadLeft(completedStr.Length + paddingSpaces + 6));

                        // cumulative time from initial transaction request (GetPayment)
                        ProcessCumulativeTime(transactionStartTime, requestStartTime, item.Value.time);
                    }
                }
                else if (waitingToComplete && item.Value.message.Contains("Request completed."))
                {
                    waitingToComplete = false;
                    TimeSpan requestDuration = DateTime.Parse(item.Value.time).Subtract(DateTime.Parse(requestStartTime));
                    string completedStr = $"COMPLETED_: {requestDuration.ToString(@"mm\:ss\.FFF").PadRight(9, '0')}";
                    Console.WriteLine(completedStr.PadLeft(completedStr.Length + paddingSpaces + 6));

                    // cumulative time from initial transaction request (GetPayment)
                    ProcessCumulativeTime(transactionStartTime, requestStartTime, item.Value.time);
                }
            }

            // overall duration
            Console.WriteLine("-".PadRight(45, '-'));
            Console.WriteLine($"TOTAL DAL PROCESSING TIME ====> : {transactionOverallTime.ToString(@"mm\:ss\.FFF").PadRight(9, '0')}");
            Console.WriteLine("-".PadRight(45, '-'));
            Console.WriteLine();
        }

        private static void ProcessCumulativeTime(string transactionStartTime, string requestStartTime, string requestDurationTime)
        {
            // cumulative time from initial transaction request (GetPayment)
            TimeSpan cumulativeDuration = DateTime.Parse(requestDurationTime).Subtract(DateTime.Parse(transactionStartTime));
            string cumulativeStr = $"CUMULATIVE: {cumulativeDuration.ToString(@"mm\:ss\.FFF").PadRight(9, '0')}";
            Console.WriteLine(cumulativeStr.PadLeft(cumulativeStr.Length + paddingSpaces + 6));
            // time between requests
            requestCumulativeTime = requestCumulativeTime.Add(TimeSpan.Parse(requestDurationTime).Subtract(TimeSpan.Parse(requestStartTime)));
            // transaction overall time
            transactionOverallTime = TimeSpan.Parse(requestStartTime).Subtract(TimeSpan.Parse(transactionStartTime));
        }
    }
}
