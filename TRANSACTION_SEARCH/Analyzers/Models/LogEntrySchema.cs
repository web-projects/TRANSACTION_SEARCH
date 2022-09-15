namespace TRANSACTION_SEARCH.Analyzers.Models
{
    public class LogEntrySchema
    {
        public string DateStamp { get; set; }
        public string TimeStamp { get; set; }
        public string UTCOffset { get; set; }
        public string LogLevel { get; set; }
        public string Assembly { get; set; }
        public string Workstation { get; set; }
        public string Message { get; set; }
    }
}
