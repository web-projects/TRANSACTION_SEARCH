using TRANSACTION_SEARCH.Helpers;

namespace TRANSACTION_SEARCH.Config
{
    public enum PaymentSegment
    {
        [StringValue("RequestId")]
        RequestId,
        [StringValue("GUID")]
        GUID
    }
}
