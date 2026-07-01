namespace TBankNet.Invoicing;

/// <summary>
/// Результат выставления счета (<c>POST /api/v1/invoice/send</c>).
/// </summary>
public sealed record TBankInvoiceSendResult : TBankInvoiceResponse
{
    /// <summary>
    /// Ссылка на PDF выставленного счета.
    /// </summary>
    /// <remarks>
    /// Действительна в течение 10 дней.
    /// </remarks>
    public required string PdfUrl { get; init; }

    /// <summary>
    /// Идентификатор выставленного счета.
    /// </summary>
    public required string InvoiceId { get; init; }

    /// <summary>
    /// Ссылка на оплату через личный кабинет Т-Бизнеса.
    /// </summary>
    public string? IncomingInvoiceUrl { get; init; }
}
