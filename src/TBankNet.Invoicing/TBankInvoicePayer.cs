namespace TBankNet.Invoicing;

/// <summary>
/// Информация о плательщике счета.
/// </summary>
public sealed record TBankInvoicePayer
{
    /// <summary>
    /// Наименование плательщика.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// ИНН плательщика.
    /// </summary>
    public string? Inn { get; init; }

    /// <summary>
    /// КПП плательщика.
    /// </summary>
    public string? Kpp { get; init; }
}
