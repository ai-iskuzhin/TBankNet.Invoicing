using System.Text.Json.Serialization;

namespace TBankNet.Invoicing;

/// <summary>
/// Запрос на выставление счета (<c>POST /api/v1/invoice/send</c>).
/// </summary>
public sealed record TBankSendInvoiceRequest
{
    /// <summary>
    /// Номер счета.
    /// </summary>
    /// <remarks>
    /// От 1 до 15 цифр (<c>^\d{1,15}$</c>).
    /// </remarks>
    public required string InvoiceNumber { get; init; }

    /// <summary>
    /// Срок оплаты.
    /// </summary>
    /// <remarks>
    /// Должен быть не меньше даты выставления счета. На проводе — <c>yyyy-MM-dd</c>.
    /// </remarks>
    [JsonConverter(typeof(TBankInvoiceDateJsonConverter))]
    public DateTime? DueDate { get; init; }

    /// <summary>
    /// Дата выставления счета.
    /// </summary>
    /// <remarks>
    /// Если не указана, счет выставляется текущей датой. На проводе — <c>yyyy-MM-dd</c>.
    /// </remarks>
    [JsonConverter(typeof(TBankInvoiceDateJsonConverter))]
    public DateTime? InvoiceDate { get; init; }

    /// <summary>
    /// Рублевый расчетный счет отправителя.
    /// </summary>
    /// <remarks>
    /// 20 или 22 цифры. Если не указан, используется главный счет компании.
    /// </remarks>
    public string? AccountNumber { get; init; }

    /// <summary>
    /// Информация о плательщике.
    /// </summary>
    public TBankInvoicePayer? Payer { get; init; }

    /// <summary>
    /// Позиции счета.
    /// </summary>
    /// <remarks>
    /// Не более 100 элементов.
    /// </remarks>
    public IReadOnlyList<TBankInvoiceItem>? Items { get; init; }

    /// <summary>
    /// Контакты для получения счета.
    /// </summary>
    /// <remarks>
    /// Не более 10 элементов.
    /// </remarks>
    public IReadOnlyList<TBankInvoiceContact>? Contacts { get; init; }

    /// <summary>
    /// Номер мобильного телефона, на который придет СМС со счетом.
    /// </summary>
    /// <remarks>
    /// Формат <c>+7XXXXXXXXXX</c> (<c>^((\+7)([0-9]){10})$</c>).
    /// </remarks>
    public string? ContactPhone { get; init; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    /// <remarks>
    /// Не более 1000 символов. Здесь можно указать условия счета-договора.
    /// </remarks>
    public string? Comment { get; init; }

    /// <summary>
    /// Текст, который попадет в назначение платежа.
    /// </summary>
    /// <remarks>
    /// Не более 512 символов. Используется при сканировании QR-кода или оплате через личный кабинет Т-Бизнеса.
    /// </remarks>
    public string? CustomPaymentPurpose { get; init; }
}
