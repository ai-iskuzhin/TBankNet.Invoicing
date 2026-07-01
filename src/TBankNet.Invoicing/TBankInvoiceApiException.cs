using System.Net;

namespace TBankNet.Invoicing;

/// <summary>
/// Ошибка API T-Bank: ответ со статусом вне диапазона 2xx.
/// </summary>
/// <remarks>
/// Несет разобранное тело ошибки (<see cref="Error"/>) и HTTP-статус.
/// </remarks>
public sealed class TBankInvoiceApiException : TBankInvoiceException
{
    /// <summary>
    /// Создает исключение API T-Bank.
    /// </summary>
    /// <param name="message">Сообщение исключения.</param>
    /// <param name="httpStatusCode">HTTP-статус ответа.</param>
    /// <param name="error">Разобранное тело ошибки T-API, если оно доступно.</param>
    /// <param name="requestId">Значение заголовка <c>X-Request-Id</c>.</param>
    public TBankInvoiceApiException(
        string message,
        HttpStatusCode httpStatusCode,
        TBankInvoiceError? error,
        string? requestId)
        : base(message)
    {
        HttpStatusCode = httpStatusCode;
        Error = error;
        RequestId = requestId;
    }

    /// <summary>
    /// HTTP-статус ответа.
    /// </summary>
    public HttpStatusCode HttpStatusCode { get; }

    /// <summary>
    /// Разобранное тело ошибки T-API.
    /// </summary>
    public TBankInvoiceError? Error { get; }

    /// <summary>
    /// Код ошибки T-Bank.
    /// </summary>
    /// <remarks>
    /// Короткий доступ к <see cref="TBankInvoiceError.ErrorCode"/>.
    /// </remarks>
    public string? ErrorCode => Error?.ErrorCode;

    /// <summary>
    /// Значение заголовка <c>X-Request-Id</c>.
    /// </summary>
    public string? RequestId { get; }
}
