using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

namespace TBankNet.Invoicing.Tests;

/// <summary>
/// Интеграционные тесты против живой песочницы T-API (<c>https://business.tbank.ru/openapi/sandbox/</c>).
/// </summary>
/// <remarks>
/// <para>
/// Тесты обращаются к внешнему сервису, поэтому по умолчанию пропускаются. Включить их все:
/// <c>TBANK_SANDBOX_TESTS=1 dotnet test --filter Category=Integration</c>.
/// </para>
/// <para>
/// Токен не требуется: используется публичный тестовый токен песочницы <c>TBankSandboxToken</c>,
/// который возвращает предопределенные ответы. Его можно переопределить переменной
/// окружения <c>TBANK_SANDBOX_TOKEN</c>.
/// </para>
/// </remarks>
[Trait("Category", "Integration")]
public sealed class SandboxIntegrationTests
{
    // Заведомо недействительный токен: доступность и трассировку можно проверить и без валидных учеток —
    // сервер отвечает 401 UNAUTHORIZED, но при этом отдает трассировочные заголовки.
    private const string InvalidToken = "invalid-token-for-availability-probe";

    // Публичный тестовый токен песочницы: возвращает предопределенные ответы (см. документацию T-API).
    private const string CannedSandboxToken = "TBankSandboxToken";

    // Произвольный корректно оформленный UUID: счета с таким id нет, но эндпоинт /info доступен для проверки.
    private const string SampleInvoiceId = "d8327c28-4a8e-4084-93ea-a94b7bd144c5";

    private static readonly Uri SandboxBaseAddress = new("https://business.tbank.ru/openapi/sandbox/");

    private static TimeSpan NetworkTimeout => TimeSpan.FromSeconds(30);

    // Реальный токен из окружения, иначе — публичный тестовый токен песочницы.
    private static string SandboxToken =>
        Environment.GetEnvironmentVariable("TBANK_SANDBOX_TOKEN") is { Length: > 0 } token
            ? token
            : CannedSandboxToken;

    private static TBankInvoiceClient CreateSandboxClient(string token) =>
        new(
            new HttpClient(),
            new TBankInvoiceClientOptions
            {
                ApiToken = token,
                Environment = TBankInvoiceEnvironment.Sandbox,
                CaptureRawResponseBody = true,
            });

    // --- Доступность песочницы (без токена) ------------------------------------------------------

    [SandboxFact]
    public async Task Send_endpoint_is_reachable_and_challenges_invalid_token()
    {
        using var cts = new CancellationTokenSource(NetworkTimeout);
        var client = CreateSandboxClient(InvalidToken);

        var exception = await Assert.ThrowsAsync<TBankInvoiceApiException>(() =>
            client.SendInvoiceAsync(
                new TBankSendInvoiceRequest
                {
                    InvoiceNumber = "1",
                    Items = new[] { new TBankInvoiceItem { Name = "probe", Price = 1m, Amount = 1m, Vat = TBankInvoiceVat.None } },
                },
                cancellationToken: cts.Token));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.HttpStatusCode);
        Assert.Equal("UNAUTHORIZED", exception.ErrorCode);
    }

    [SandboxFact]
    public async Task Info_endpoint_is_reachable_and_challenges_invalid_token()
    {
        using var cts = new CancellationTokenSource(NetworkTimeout);
        var client = CreateSandboxClient(InvalidToken);

        var exception = await Assert.ThrowsAsync<TBankInvoiceApiException>(() =>
            client.GetInvoiceAsync(SampleInvoiceId, cancellationToken: cts.Token));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.HttpStatusCode);
        Assert.Equal("UNAUTHORIZED", exception.ErrorCode);
        // Тело ошибки соответствует схеме T-API и корректно разбирается клиентом.
        Assert.False(string.IsNullOrWhiteSpace(exception.Error!.ErrorId));
    }

    // --- Трассировочные заголовки (x-request-id) -------------------------------------------------

    [SandboxFact]
    public async Task Client_surfaces_supplied_request_id_from_error_response()
    {
        using var cts = new CancellationTokenSource(NetworkTimeout);
        var client = CreateSandboxClient(InvalidToken);
        var requestId = "tbanknet-int-" + Guid.NewGuid().ToString("N");

        var exception = await Assert.ThrowsAsync<TBankInvoiceApiException>(() =>
            client.GetInvoiceAsync(SampleInvoiceId, requestId, cts.Token));

        // Сервер эхом возвращает X-Request-Id, а клиент прокидывает его в исключение — сквозная трассировка.
        Assert.Equal(requestId, exception.RequestId);
    }

    [SandboxFact]
    public async Task Response_carries_x_request_id_tracing_header()
    {
        using var cts = new CancellationTokenSource(NetworkTimeout);
        using var http = new HttpClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(SandboxBaseAddress, $"api/v1/openapi/invoice/{SampleInvoiceId}/info"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", InvalidToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await http.SendAsync(request, cts.Token);

        // Основной корреляционный заголовок трассировки присутствует даже на пути ошибки (401).
        Assert.True(
            response.Headers.TryGetValues("x-request-id", out var requestIds) &&
                requestIds.Any(value => !string.IsNullOrWhiteSpace(value)),
            "Ожидался трассировочный заголовок x-request-id в ответе песочницы.");

        // Сервер сам генерирует X-Request-Id, если клиент его не прислал (значение — валидный GUID).
        Assert.True(
            Guid.TryParse(response.Headers.GetValues("x-request-id").First(), out _),
            "Сервер должен вернуть x-request-id в виде GUID, когда клиент его не задал.");
    }

    // --- Полный сценарий: выставить счет и получить его статус ------------------------------------

    [SandboxFact]
    public async Task Send_then_get_invoice_roundtrip_exposes_tracing_metadata()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var client = CreateSandboxClient(SandboxToken);

        // Псевдоуникальный номер счета в пределах 15 цифр (unix-секунды — 10 цифр).
        var invoiceNumber = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

        var sent = await client.SendInvoiceAsync(
            new TBankSendInvoiceRequest
            {
                InvoiceNumber = invoiceNumber,
                Items = new[]
                {
                    new TBankInvoiceItem
                    {
                        Name = "Интеграционный тест",
                        Price = 100m,
                        Amount = 1m,
                        Unit = "шт",
                        Vat = TBankInvoiceVat.None,
                    },
                },
            },
            cancellationToken: cts.Token);

        Assert.False(string.IsNullOrWhiteSpace(sent.InvoiceId));
        Assert.False(string.IsNullOrWhiteSpace(sent.PdfUrl));

        // На успешном пути метаданные несут трассировочные данные.
        Assert.NotNull(sent.Metadata);
        Assert.Equal(HttpStatusCode.OK, sent.Metadata!.HttpStatusCode);
        Assert.False(string.IsNullOrWhiteSpace(sent.Metadata.RequestId));
        Assert.True(sent.Metadata.Headers.ContainsKey("x-request-id"));

        var info = await client.GetInvoiceAsync(sent.InvoiceId, cancellationToken: cts.Token);

        Assert.NotEqual(TBankInvoiceStatus.Unknown, info.Status);
        Assert.NotNull(info.Metadata);
        Assert.True(info.Metadata!.Headers.ContainsKey("x-request-id"));
    }

    [SandboxFact]
    public async Task Send_without_item_unit_returns_validation_error_with_details()
    {
        using var cts = new CancellationTokenSource(NetworkTimeout);
        var client = CreateSandboxClient(SandboxToken);
        var invoiceNumber = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

        // Сервер требует "unit" у каждой позиции; модель SDK помечает Unit как необязательный,
        // а WhenWritingNull опускает его в JSON — сервер отвечает 400 VALIDATION_ERROR.
        var exception = await Assert.ThrowsAsync<TBankInvoiceApiException>(() =>
            client.SendInvoiceAsync(
                new TBankSendInvoiceRequest
                {
                    InvoiceNumber = invoiceNumber,
                    Items = new[] { new TBankInvoiceItem { Name = "Без единицы", Price = 100m, Amount = 1m } },
                },
                cancellationToken: cts.Token));

        Assert.Equal(HttpStatusCode.BadRequest, exception.HttpStatusCode);
        Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
        // errorDetails заполняется и корректно разбирается в JsonElement.
        Assert.NotNull(exception.Error!.ErrorDetails);
        Assert.Equal(System.Text.Json.JsonValueKind.Object, exception.Error.ErrorDetails!.Value.ValueKind);
    }
}
