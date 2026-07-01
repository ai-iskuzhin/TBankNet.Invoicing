namespace TBankNet.Invoicing;

/// <summary>
/// Контакт для получения счета.
/// </summary>
public sealed record TBankInvoiceContact
{
    /// <summary>
    /// Адрес электронной почты.
    /// </summary>
    public string? Email { get; init; }
}
