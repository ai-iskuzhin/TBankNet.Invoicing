namespace TBankNet.Invoicing;

/// <summary>
/// Настройки клиента T-API выставления счетов.
/// </summary>
/// <example>
/// <code>
/// var client = new TBankInvoiceClient(httpClient, new TBankInvoiceClientOptions
/// {
///     ApiToken = "YOUR_API_TOKEN",
///     Environment = TBankInvoiceEnvironment.Sandbox
/// });
/// </code>
/// </example>
public sealed class TBankInvoiceClientOptions
{
    /// <summary>
    /// API-токен T-Bank для авторизации по схеме Bearer.
    /// </summary>
    /// <remarks>
    /// Отправляется в заголовке <c>Authorization: Bearer &lt;token&gt;</c> в каждом запросе.
    /// </remarks>
    public required string ApiToken { get; init; }

    /// <summary>
    /// Среда T-API, используемая при отсутствии <see cref="BaseAddress"/>.
    /// </summary>
    /// <remarks>
    /// По умолчанию <see cref="TBankInvoiceEnvironment.Production"/>.
    /// </remarks>
    public TBankInvoiceEnvironment Environment { get; init; } = TBankInvoiceEnvironment.Production;

    /// <summary>
    /// Явный базовый адрес T-API, если нужно переопределить среду.
    /// </summary>
    /// <remarks>
    /// Например, <c>https://business.tbank.ru/openapi/</c>. <br/>
    /// Может не заканчиваться на «/» — клиент нормализует адрес.
    /// </remarks>
    public Uri? BaseAddress { get; init; }

    /// <summary>
    /// Сохранять сырое тело ответа в <see cref="TBankInvoiceResponseMetadata.RawResponseBody"/>.
    /// </summary>
    /// <remarks>
    /// false по умолчанию, чтобы случайно не хранить персональные данные плательщика.
    /// </remarks>
    public bool CaptureRawResponseBody { get; init; }

    internal Uri ResolveBaseAddress()
    {
        if (BaseAddress is not null)
        {
            return BaseAddress.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
                ? BaseAddress
                : new Uri(BaseAddress.AbsoluteUri + "/");
        }

        return Environment switch
        {
            TBankInvoiceEnvironment.Production => new Uri("https://business.tbank.ru/openapi/"),
            TBankInvoiceEnvironment.Sandbox => new Uri("https://business.tbank.ru/openapi/sandbox/"),
            _ => throw new InvalidOperationException($"Unsupported invoicing environment: {Environment}.")
        };
    }
}
