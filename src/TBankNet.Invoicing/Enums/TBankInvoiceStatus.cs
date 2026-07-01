namespace TBankNet.Invoicing;

/// <summary>
/// Статус выставленного счета.
/// </summary>
public enum TBankInvoiceStatus
{
    /// <summary>
    /// Значение по умолчанию (статус не задан).
    /// </summary>
    /// <remarks>
    /// API это значение не возвращает.
    /// </remarks>
    Unknown,

    /// <summary>
    /// Черновик.
    /// </summary>
    /// <remarks>
    /// Проводное значение <c>DRAFT</c>.
    /// </remarks>
    Draft,

    /// <summary>
    /// Отправлен.
    /// </summary>
    /// <remarks>
    /// Проводное значение <c>SUBMITTED</c>.
    /// </remarks>
    Submitted,

    /// <summary>
    /// Оплачен.
    /// </summary>
    /// <remarks>
    /// Проводное значение <c>EXECUTED</c>.
    /// </remarks>
    Executed
}
