namespace TBankNet.Invoicing;

/// <summary>
/// Среда T-API T-Bank, в которую отправляются запросы выставления счетов.
/// </summary>
/// <remarks>
/// Базовый адрес можно переопределить через <see cref="TBankInvoiceClientOptions.BaseAddress"/>,
/// если требуется нестандартный хост.
/// </remarks>
public enum TBankInvoiceEnvironment
{
    /// <summary>
    /// Боевая среда.
    /// </summary>
    /// <remarks>
    /// Соответствует <c>https://business.tbank.ru/openapi/</c>.
    /// </remarks>
    Production,

    /// <summary>
    /// Песочница (Sandbox).
    /// </summary>
    /// <remarks>
    /// Соответствует <c>https://business.tbank.ru/openapi/sandbox/</c>.
    /// </remarks>
    Sandbox
}
