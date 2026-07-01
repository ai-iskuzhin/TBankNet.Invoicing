using System.Net;

namespace TBankNet.Invoicing;

/// <summary>
/// Ошибка протокола: ответ получен, но его невозможно безопасно разобрать как ответ T-API.
/// </summary>
/// <remarks>
/// Например, HTML-страница, пустое тело или JSON неожиданной формы.
/// </remarks>
public sealed class TBankInvoiceProtocolException : TBankInvoiceException
{
    /// <summary>
    /// Создает исключение протокола T-API.
    /// </summary>
    /// <param name="message">Сообщение исключения.</param>
    /// <param name="httpStatusCode">HTTP-статус ответа.</param>
    /// <param name="requestId">Значение заголовка <c>X-Request-Id</c>.</param>
    /// <param name="responseBodyPreview">Короткий отредактированный фрагмент тела ответа.</param>
    /// <param name="innerException">Исходная ошибка разбора ответа.</param>
    public TBankInvoiceProtocolException(
        string message,
        HttpStatusCode? httpStatusCode = null,
        string? requestId = null,
        string? responseBodyPreview = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        HttpStatusCode = httpStatusCode;
        RequestId = requestId;
        ResponseBodyPreview = responseBodyPreview;
    }

    /// <summary>
    /// HTTP-статус ответа, если он был получен.
    /// </summary>
    public HttpStatusCode? HttpStatusCode { get; }

    /// <summary>
    /// Значение заголовка <c>X-Request-Id</c>.
    /// </summary>
    public string? RequestId { get; }

    /// <summary>
    /// Короткий отредактированный фрагмент тела ответа для диагностики.
    /// </summary>
    public string? ResponseBodyPreview { get; }
}
