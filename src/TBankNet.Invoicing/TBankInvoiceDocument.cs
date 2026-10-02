namespace TBankNet.Invoicing;

/// <summary>
/// Файл счета, скачанный по ссылке <see cref="TBankInvoiceSendResult.PdfUrl"/>.
/// </summary>
/// <remarks>
/// Ссылка ведет на публичный документ (<c>/invoices/api/v1/public/document/{token}</c>) и не требует
/// авторизации — секрет в самом токене ссылки. Поэтому запрос уходит без Bearer-токена, см.
/// <see cref="TBankInvoiceClient.GetInvoiceDocumentAsync"/>.
/// </remarks>
public sealed record TBankInvoiceDocument : TBankInvoiceResponse
{
    /// <summary>Содержимое файла.</summary>
    public required byte[] Content { get; init; }

    /// <summary>MIME-тип из заголовка <c>Content-Type</c>; у счета это <c>application/pdf</c>.</summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Имя файла из заголовка <c>Content-Disposition</c>, если банк его прислал.
    /// </summary>
    /// <remarks>
    /// T-Bank присылает его в кодировке RFC 5987 (<c>filename*=UTF-8''…</c>) и по-русски — например
    /// <c>Счет № 3 от 29.09.26.pdf</c>. Значение раскодировано; если заголовка нет, здесь null и имя
    /// файла придумывает вызывающий код.
    /// </remarks>
    public string? FileName { get; init; }
}
