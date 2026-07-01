using System.Text.Json.Serialization;

namespace TBankNet.Invoicing;

/// <summary>
/// Информация о выставленном счете (<c>GET /api/v1/openapi/invoice/{invoiceId}/info</c>).
/// </summary>
public sealed record TBankInvoiceInfo : TBankInvoiceResponse
{
    /// <summary>
    /// Статус выставленного счета.
    /// </summary>
    [JsonConverter(typeof(TBankInvoiceStatusJsonConverter))]
    public TBankInvoiceStatus Status { get; init; }
}
