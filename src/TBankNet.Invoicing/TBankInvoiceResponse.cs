using System.Text.Json.Serialization;

namespace TBankNet.Invoicing;

/// <summary>
/// Базовый ответ T-API выставления счетов.
/// </summary>
public abstract record TBankInvoiceResponse
{
    /// <summary>
    /// HTTP-метаданные ответа.
    /// </summary>
    /// <remarks>
    /// Статус, заголовки и X-Request-Id. Не входит в тело ответа T-API, заполняется клиентом.
    /// </remarks>
    [JsonIgnore]
    public TBankInvoiceResponseMetadata? Metadata { get; internal set; }
}
