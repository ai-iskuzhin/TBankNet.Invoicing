using System.Text.Json;

namespace TBankNet.Invoicing;

/// <summary>
/// Тело ошибки T-API для статусов вне диапазона 2xx.
/// </summary>
/// <remarks>
/// Соответствует схеме ошибки методов выставления счетов: <c>errorCode</c>, <c>errorId</c>,
/// <c>errorMessage</c> и необязательные <c>errorDetails</c>.
/// </remarks>
public sealed record TBankInvoiceError
{
    /// <summary>
    /// Код ошибки.
    /// </summary>
    /// <remarks>
    /// Не более 50 символов.
    /// </remarks>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Уникальный идентификатор ошибки.
    /// </summary>
    /// <remarks>
    /// Не более 50 символов. Полезен для обращения в поддержку.
    /// </remarks>
    public string? ErrorId { get; init; }

    /// <summary>
    /// Текст ошибки.
    /// </summary>
    /// <remarks>
    /// Не более 400 символов.
    /// </remarks>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Дополнительные данные об ошибке.
    /// </summary>
    /// <remarks>
    /// Произвольный объект; сохраняется как <see cref="JsonElement"/> для доступа без предопределенной схемы.
    /// </remarks>
    public JsonElement? ErrorDetails { get; init; }
}
