namespace TBankNet.Invoicing;

/// <summary>
/// Ошибка локальной валидации запроса до отправки в T-API.
/// </summary>
/// <remarks>
/// Используется для очевидно некорректных запросов: пустой номер счета, неверный формат телефона, срок оплаты раньше даты счета.
/// </remarks>
public sealed class TBankInvoiceValidationException : TBankInvoiceException
{
    /// <summary>
    /// Создает исключение локальной валидации.
    /// </summary>
    /// <param name="message">Описание ошибки валидации.</param>
    public TBankInvoiceValidationException(string message)
        : base(message)
    {
    }
}
