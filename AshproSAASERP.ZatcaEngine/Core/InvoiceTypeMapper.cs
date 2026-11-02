using AshproSAASERP.ZatcaEngine.Enums;

namespace AshproSAASERP.ZatcaEngine.Core;

public static class InvoiceTypeMapper
{
    public static string GetUblCode(InvoiceType type)
    {
        return type switch
        {
            InvoiceType.StandardInvoice => "388",
            InvoiceType.SimplifiedInvoice => "388",

            InvoiceType.StandardCreditNote => "381",
            InvoiceType.SimplifiedCreditNote => "381",

            InvoiceType.StandardDebitNote => "383",
            InvoiceType.SimplifiedDebitNote => "383",

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static bool RequiresClearance(InvoiceType type)
    {
        return type switch
        {
            InvoiceType.StandardInvoice => true,
            InvoiceType.StandardCreditNote => true,
            InvoiceType.StandardDebitNote => true,

            InvoiceType.SimplifiedInvoice => false,
            InvoiceType.SimplifiedCreditNote => false,
            InvoiceType.SimplifiedDebitNote => false,

            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
