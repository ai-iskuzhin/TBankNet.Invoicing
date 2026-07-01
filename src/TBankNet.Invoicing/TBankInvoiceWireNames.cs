using System.Globalization;

namespace TBankNet.Invoicing;

/// <summary>
/// Преобразование типизированных enum в проводные строковые значения T-API.
/// </summary>
internal static class TBankInvoiceWireNames
{
    public static string FormatStatus(TBankInvoiceStatus value)
    {
        return value switch
        {
            TBankInvoiceStatus.Draft => "DRAFT",
            TBankInvoiceStatus.Submitted => "SUBMITTED",
            TBankInvoiceStatus.Executed => "EXECUTED",
            _ => throw new InvalidOperationException($"Unknown invoice status: {value}.")
        };
    }

    public static string FormatVat(TBankInvoiceVat value)
    {
        if (value == TBankInvoiceVat.None)
        {
            return "None";
        }

        if (!Enum.IsDefined(typeof(TBankInvoiceVat), value))
        {
            throw new InvalidOperationException($"Unknown invoice VAT rate: {value}.");
        }

        // Для процентных ставок целочисленное значение enum совпадает с процентом; провод ожидает строку ("22").
        return ((int)value).ToString(CultureInfo.InvariantCulture);
    }
}
