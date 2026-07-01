using System.Text.Json.Serialization;

namespace TBankNet.Invoicing;

/// <summary>
/// Позиция счета.
/// </summary>
public sealed record TBankInvoiceItem
{
    /// <summary>
    /// Наименование позиции.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Цена за единицу.
    /// </summary>
    public required decimal Price { get; init; }

    /// <summary>
    /// Единица измерения.
    /// </summary>
    /// <remarks>
    /// Например, «Шт».
    /// </remarks>
    public string? Unit { get; init; }

    /// <summary>
    /// Ставка НДС.
    /// </summary>
    /// <remarks>
    /// Без НДС (<see cref="TBankInvoiceVat.None"/>) или процент. На проводе — строка.
    /// </remarks>
    [JsonConverter(typeof(TBankInvoiceVatJsonConverter))]
    public TBankInvoiceVat? Vat { get; init; }

    /// <summary>
    /// Количество.
    /// </summary>
    public required decimal Amount { get; init; }
}
