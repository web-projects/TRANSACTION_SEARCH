namespace TRANSACTION_SEARCH.Analyzers.Models
{
    public enum RequestSchemaIndex : int
    {
        GetPayment = 0,
        GetCreditOrDebit = 1,
        GetPIN = 2,
        GetZIP = 3,
        GetVerifyAmount = 4,
        RemoveCard = 5,
        DeviceUI = 6,
        ADAMode = 7,
    }
}
