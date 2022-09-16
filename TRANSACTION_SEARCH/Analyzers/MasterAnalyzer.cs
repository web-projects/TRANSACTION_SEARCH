using System.Collections.Generic;

namespace TRANSACTION_SEARCH.Analyzers
{
    public static class MasterAnalyzer
    {
        public static void Analyze(string guid, string request, List<string> transactionLog)
        {
            ServicerCore.Analyze(guid, request, transactionLog);
            DALCore.Analyze(guid, transactionLog);
        }
    }
}
