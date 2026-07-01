using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TBankNet.Invoicing;

/// <summary>
/// Клиент T-API T-Bank для выставления счетов на оплату (Выставление счетов).
/// </summary>
/// <remarks>
/// Методы: выставить счет и получить его статус. Авторизация — Bearer API-токен. Для каждого запроса
/// генерируется заголовок <c>X-Request-Id</c> (его можно задать явно). Ответы со статусом вне диапазона
/// 2xx приводят к <see cref="TBankInvoiceApiException"/>.
/// </remarks>
public sealed class TBankInvoiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private static readonly string UserAgent = BuildUserAgent();

    private readonly HttpClient httpClient;
    private readonly TBankInvoiceClientOptions options;

    /// <summary>
    /// Создает клиент выставления счетов.
    /// </summary>
    /// <param name="httpClient">HTTP-клиент. Может управляться <c>IHttpClientFactory</c>.</param>
    /// <param name="options">Настройки клиента.</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="httpClient"/> или <paramref name="options"/> равны null.</exception>
    /// <exception cref="ArgumentException">Если не задан <see cref="TBankInvoiceClientOptions.ApiToken"/>.</exception>
    public TBankInvoiceClient(HttpClient httpClient, TBankInvoiceClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ApiToken))
        {
            throw new ArgumentException("API token must be configured.", nameof(options));
        }

        this.httpClient = httpClient;
        this.options = options;
    }

    /// <summary>
    /// Выставляет счет на оплату и возвращает ссылку на PDF и идентификатор счета.
    /// </summary>
    /// <remarks>
    /// Метод <c>POST /api/v1/invoice/send</c>. Ограничение — 4 запроса в секунду.
    /// </remarks>
    /// <param name="request">Параметры счета.</param>
    /// <param name="requestId">Значение заголовка <c>X-Request-Id</c>. Если null, генерируется автоматически.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выставления счета.</returns>
    /// <exception cref="TBankInvoiceValidationException">Если запрос не проходит локальную валидацию.</exception>
    /// <exception cref="TBankInvoiceApiException">Если T-API вернул статус вне диапазона 2xx.</exception>
    /// <exception cref="TBankInvoiceTransportException">Если ответ не получен из-за ошибки транспорта.</exception>
    /// <exception cref="TBankInvoiceProtocolException">Если ответ невозможно разобрать.</exception>
    public Task<TBankInvoiceSendResult> SendInvoiceAsync(
        TBankSendInvoiceRequest request,
        string? requestId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        TBankInvoiceRequestValidator.Validate(request);

        return SendAsync<TBankSendInvoiceRequest, TBankInvoiceSendResult>(
            HttpMethod.Post, "api/v1/invoice/send", request, requestId, cancellationToken);
    }

    /// <summary>
    /// Возвращает статус ранее выставленного счета.
    /// </summary>
    /// <remarks>
    /// Метод <c>GET /api/v1/openapi/invoice/{invoiceId}/info</c>. Ограничение — 20 запросов в секунду.
    /// </remarks>
    /// <param name="invoiceId">Идентификатор выставленного счета.</param>
    /// <param name="requestId">Значение заголовка <c>X-Request-Id</c>. Если null, генерируется автоматически.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Информация о счете со статусом.</returns>
    /// <exception cref="TBankInvoiceValidationException">Если <paramref name="invoiceId"/> пуст.</exception>
    /// <exception cref="TBankInvoiceApiException">Если T-API вернул статус вне диапазона 2xx.</exception>
    /// <exception cref="TBankInvoiceTransportException">Если ответ не получен из-за ошибки транспорта.</exception>
    /// <exception cref="TBankInvoiceProtocolException">Если ответ невозможно разобрать.</exception>
    public Task<TBankInvoiceInfo> GetInvoiceAsync(
        string invoiceId,
        string? requestId = null,
        CancellationToken cancellationToken = default)
    {
        TBankInvoiceRequestValidator.ValidateInvoiceId(invoiceId);

        // Внимание: у метода info путь содержит дополнительный сегмент "openapi" (в отличие от "send").
        var path = $"api/v1/openapi/invoice/{Uri.EscapeDataString(invoiceId)}/info";

        return SendAsync<object?, TBankInvoiceInfo>(HttpMethod.Get, path, body: null, requestId, cancellationToken);
    }

    private async Task<TResponse> SendAsync<TRequest, TResponse>(
        HttpMethod method,
        string relativePath,
        TRequest? body,
        string? requestId,
        CancellationToken cancellationToken)
        where TResponse : TBankInvoiceResponse
    {
        var endpoint = new Uri(options.ResolveBaseAddress(), relativePath);
        var effectiveRequestId = string.IsNullOrWhiteSpace(requestId) ? Guid.NewGuid().ToString() : requestId!;
        HttpResponseMessage response;

        try
        {
            using var httpRequest = new HttpRequestMessage(method, endpoint);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiToken);
            httpRequest.Headers.TryAddWithoutValidation("X-Request-Id", effectiveRequestId);
            httpRequest.Headers.UserAgent.ParseAdd(UserAgent);
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (body is not null)
            {
                httpRequest.Content = JsonContent.Create(body, body.GetType(), options: JsonOptions);
            }

            response = await httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException exception)
        {
            throw new TBankInvoiceTransportException(
                $"T-Bank {method} {relativePath} request failed before a response was received.",
                exception);
        }

        using (response)
        {
#if NETSTANDARD2_0
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
            var responseRequestId = ReadRequestId(response, effectiveRequestId);

            if (!response.IsSuccessStatusCode)
            {
                throw CreateApiException(method, relativePath, response.StatusCode, responseRequestId, responseBody);
            }

            var result = DeserializeBody<TResponse>(method, relativePath, response.StatusCode, responseRequestId, responseBody);
            result.Metadata = CreateResponseMetadata(response, responseRequestId, responseBody, options.CaptureRawResponseBody);

            return result;
        }
    }

    private static TBankInvoiceApiException CreateApiException(
        HttpMethod method,
        string relativePath,
        System.Net.HttpStatusCode statusCode,
        string? requestId,
        string responseBody)
    {
        TBankInvoiceError? error = null;
        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            try
            {
                error = JsonSerializer.Deserialize<TBankInvoiceError>(responseBody, JsonOptions);
            }
            catch (JsonException)
            {
                // Тело ошибки не соответствует ожидаемой схеме — оставляем error = null.
            }
        }

        var suffix = error?.ErrorCode is { Length: > 0 } code ? $" ErrorCode '{code}'." : ".";

        return new TBankInvoiceApiException(
            $"T-Bank {method} {relativePath} returned HTTP {(int)statusCode} ({statusCode}){suffix}",
            statusCode,
            error,
            requestId);
    }

    private static TResponse DeserializeBody<TResponse>(
        HttpMethod method,
        string relativePath,
        System.Net.HttpStatusCode statusCode,
        string? requestId,
        string responseBody)
        where TResponse : TBankInvoiceResponse
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            throw new TBankInvoiceProtocolException(
                $"T-Bank {method} {relativePath} response body was empty. HTTP {(int)statusCode} ({statusCode}).",
                statusCode,
                requestId);
        }

        try
        {
            return JsonSerializer.Deserialize<TResponse>(responseBody, JsonOptions)
                ?? throw new TBankInvoiceProtocolException(
                    $"T-Bank {method} {relativePath} response body was null after deserialization.",
                    statusCode,
                    requestId,
                    CreateBodyPreview(responseBody));
        }
        catch (JsonException exception)
        {
            var preview = CreateBodyPreview(responseBody);
            throw new TBankInvoiceProtocolException(
                $"T-Bank {method} {relativePath} response body was not valid JSON for the expected model. HTTP {(int)statusCode} ({statusCode}). Response preview: {preview}",
                statusCode,
                requestId,
                preview,
                exception);
        }
    }

    private static string? ReadRequestId(HttpResponseMessage response, string fallback)
    {
        if (response.Headers.TryGetValues("X-Request-Id", out var values))
        {
            var value = values.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return fallback;
    }

    private static TBankInvoiceResponseMetadata CreateResponseMetadata(
        HttpResponseMessage response,
        string? requestId,
        string responseBody,
        bool captureRawResponseBody)
    {
        var headers = response.Headers
            .Concat(response.Content.Headers)
            .GroupBy(static header => header.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                static group => group.Key,
                static group => group.SelectMany(static header => header.Value).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        return new TBankInvoiceResponseMetadata(
            response.StatusCode,
            requestId,
            headers,
            captureRawResponseBody ? responseBody : null);
    }

    private static string CreateBodyPreview(string responseBody)
    {
        var preview = RedactSensitiveFields(responseBody);
        const int maxLength = 512;

        return preview.Length <= maxLength ? preview : preview.Substring(0, maxLength);
    }

    private static string RedactSensitiveFields(string value)
    {
        // Скрываем персональные/платежные данные из диагностических фрагментов.
        return Regex.Replace(
            value,
            "(\"(?:inn|kpp|accountNumber|contactPhone|email)\"\\s*:\\s*\")([^\"]*)(\")",
            "$1***REDACTED***$3",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static string BuildUserAgent()
    {
        var assembly = typeof(TBankInvoiceClient).Assembly;
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "0.0.0";

        var metadataSeparator = version.IndexOf('+');
        if (metadataSeparator >= 0)
        {
            version = version.Substring(0, metadataSeparator);
        }

        return $"TBankNet.Invoicing/{version} ({RuntimeInformation.FrameworkDescription})";
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        // Web-дефолты дают camelCase (invoiceNumber, pdfUrl, invoiceId...), как ожидает T-API.
        // Relaxed encoder не экранирует кириллицу (\uXXXX) — назначение/комментарий читаемы в теле запроса.
        return new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
}
