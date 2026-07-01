namespace TBankNet.Invoicing;

/// <summary>
/// Ставка НДС позиции счета.
/// </summary>
/// <remarks>
/// Кроме процентных ставок допускается значение «без НДС» (<see cref="None"/>). На проводе передается
/// строкой: <c>"None"</c> или процент («22»). Для процентных значений целочисленное значение enum
/// совпадает с процентом (<c>(int)TBankInvoiceVat.Vat22 == 22</c>).
/// </remarks>
public enum TBankInvoiceVat
{
    /// <summary>
    /// Без НДС.
    /// </summary>
    /// <remarks>
    /// Проводное значение <c>"None"</c>.
    /// </remarks>
    None = -1,

    /// <summary>
    /// НДС 0%.
    /// </summary>
    /// <remarks>
    /// Проводное значение <c>"0"</c>.
    /// </remarks>
    Vat0 = 0,

    /// <summary>
    /// НДС 5%.
    /// </summary>
    /// <remarks>
    /// Проводное значение <c>"5"</c>.
    /// </remarks>
    Vat5 = 5,

    /// <summary>
    /// НДС 7%.
    /// </summary>
    /// <remarks>
    /// Проводное значение <c>"7"</c>.
    /// </remarks>
    Vat7 = 7,

    /// <summary>
    /// НДС 10%.
    /// </summary>
    /// <remarks>
    /// Проводное значение <c>"10"</c>.
    /// </remarks>
    Vat10 = 10,

    /// <summary>
    /// НДС 20%.
    /// </summary>
    /// <remarks>
    /// Проводное значение <c>"20"</c>.
    /// </remarks>
    Vat20 = 20,

    /// <summary>
    /// НДС 22%.
    /// </summary>
    /// <remarks>
    /// Проводное значение <c>"22"</c>.
    /// </remarks>
    Vat22 = 22
}
