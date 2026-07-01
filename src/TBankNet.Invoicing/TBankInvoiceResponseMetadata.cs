using System.Net;

namespace TBankNet.Invoicing;

/// <summary>
/// Метаданные HTTP-ответа T-API для диагностики и трассировки.
/// </summary>
public sealed class TBankInvoiceResponseMetadata
{
    /// <summary>
    /// Создает метаданные ответа.
    /// </summary>
    /// <param name="httpStatusCode">HTTP-статус ответа.</param>
    /// <param name="requestId">Значение заголовка <c>X-Request-Id</c>.</param>
    /// <param name="headers">Заголовки ответа и тела ответа.</param>
    /// <param name="rawResponseBody">Сырое тело ответа, если включен захват.</param>
    public TBankInvoiceResponseMetadata(
        HttpStatusCode httpStatusCode,
        string? requestId,
        IReadOnlyDictionary<string, string[]> headers,
        string? rawResponseBody)
    {
        HttpStatusCode = httpStatusCode;
        RequestId = requestId;
        Headers = headers;
        RawResponseBody = rawResponseBody;
    }

    /// <summary>
    /// HTTP-статус ответа.
    /// </summary>
    public HttpStatusCode HttpStatusCode { get; }

    /// <summary>
    /// Значение заголовка <c>X-Request-Id</c>.
    /// </summary>
    /// <remarks>
    /// Идентификатор трассировки, отправленный клиентом и/или возвращенный сервером.
    /// </remarks>
    public string? RequestId { get; }

    /// <summary>
    /// Заголовки ответа и тела ответа.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Headers { get; }

    /// <summary>
    /// Сырое тело ответа.
    /// </summary>
    /// <remarks>
    /// Заполняется, только если включен <see cref="TBankInvoiceClientOptions.CaptureRawResponseBody"/>.
    /// </remarks>
    public string? RawResponseBody { get; }
}
