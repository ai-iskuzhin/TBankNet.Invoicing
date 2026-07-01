namespace TBankNet.Invoicing;

/// <summary>
/// Базовое исключение SDK TBankNet.Invoicing.
/// </summary>
/// <remarks>
/// Используется для транспортных, протокольных, локальных ошибок валидации и ошибок API T-Bank.
/// </remarks>
public abstract class TBankInvoiceException : Exception
{
    /// <summary>
    /// Создает исключение SDK.
    /// </summary>
    /// <param name="message">Сообщение исключения.</param>
    protected TBankInvoiceException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Создает исключение SDK с внутренней причиной.
    /// </summary>
    /// <param name="message">Сообщение исключения.</param>
    /// <param name="innerException">Внутренняя причина.</param>
    protected TBankInvoiceException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
