using TRANSACTION_SEARCH.Config;

namespace APP_CONFIG.Config
{
    public class FileGroup
    {
        public string Input { get; set; }
        public PaymentSegment PaymentSegment { get; set; } = PaymentSegment.RequestId;
        public string Guid { get; set; }
        public string Request { get; set; }
        public string MessageId { get; set; }
        public FilterOut FilterOut { get; set; }
    }
}
