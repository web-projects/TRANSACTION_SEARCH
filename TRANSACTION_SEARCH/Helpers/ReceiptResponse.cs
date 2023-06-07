namespace TRANSACTION_SEARCH.Helpers
{
    public class ReceiptResponse
    {
        public string CardholderPANFirst6 { get; set; }
        public string CardholderPANLast4 { get; set; }
        public string CardExpirationDate { get; set; }
        public string TransactionAmount { get; set; }
        public string ApplicationPreferredName { get; set; }
        public string PaymentNetwork { get; set; }
        public string ApplicationID { get; set; }
        public string CardEntryMethod { get; set; }
        public string CardholderVerificationMethod { get; set; }
        public string ApplicationLabel { get; set; }
        public string TransactionDate { get; set; }
    }
}
