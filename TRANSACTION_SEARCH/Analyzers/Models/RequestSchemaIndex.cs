namespace TRANSACTION_SEARCH.Analyzers.Models
{
    public enum RequestSchemaIndex : int
    {
        GetPayment = 0,
        GetCreditOrDebit = 1,
        GetPIN = 2,
        GetZIP = 3,
        RemoveCard = 4,
        DeviceUI = 5
    }
}
