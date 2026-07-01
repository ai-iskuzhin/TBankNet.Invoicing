namespace TBankNet.Invoicing;

/// <summary>
/// Ошибка транспорта: запрос не получил корректный HTTP-ответ.
/// </summary>
/// <remarks>
/// Например, DNS, TLS, сетевой сбой или ошибка HttpClient до получения ответа.
/// </remarks>
public sealed class TBankInvoiceTransportException : TBankInvoiceException
{
    /// <summary>
    /// Создает исключение транспортного уровня.
    /// </summary>
    /// <param name="message">Сообщение исключения.</param>
    /// <param name="innerException">Исходная ошибка транспорта.</param>
    public TBankInvoiceTransportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
