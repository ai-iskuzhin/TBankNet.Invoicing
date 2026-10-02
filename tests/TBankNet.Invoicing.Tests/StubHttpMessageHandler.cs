using System.Net;

namespace TBankNet.Invoicing.Tests;

/// <summary>
/// Тестовый обработчик HTTP: возвращает заранее заданный ответ и сохраняет последний запрос.
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode statusCode;
    private readonly string responseBody;
    private readonly byte[]? binaryBody;
    private readonly string? binaryContentType;
    private readonly string? contentDisposition;

    public StubHttpMessageHandler(HttpStatusCode statusCode, string responseBody)
    {
        this.statusCode = statusCode;
        this.responseBody = responseBody;
    }

    private StubHttpMessageHandler(
        HttpStatusCode statusCode, byte[] binaryBody, string? binaryContentType, string? contentDisposition)
    {
        this.statusCode = statusCode;
        this.responseBody = string.Empty;
        this.binaryBody = binaryBody;
        this.binaryContentType = binaryContentType;
        this.contentDisposition = contentDisposition;
    }

    /// <summary>
    /// Обработчик, отдающий бинарное тело — для скачивания файла счета. Заголовок
    /// <c>Content-Disposition</c> задается строкой намеренно: проверяем разбор того, что реально
    /// присылает банк (RFC 5987), а не то, что соберет типизированный хелпер.
    /// </summary>
    public static StubHttpMessageHandler Binary(
        byte[] body, string? contentType = "application/pdf", string? contentDisposition = null) =>
        new(HttpStatusCode.OK, body, contentType, contentDisposition);

    public HttpRequestMessage? LastRequest { get; private set; }

    public string? LastRequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastRequestBody = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        HttpResponseMessage response;
        if (binaryBody is not null)
        {
            response = new HttpResponseMessage(statusCode) { Content = new ByteArrayContent(binaryBody) };
            response.Content.Headers.ContentType = binaryContentType is null
                ? null
                : new System.Net.Http.Headers.MediaTypeHeaderValue(binaryContentType);
            if (contentDisposition is not null)
            {
                response.Content.Headers.TryAddWithoutValidation("Content-Disposition", contentDisposition);
            }
        }
        else
        {
            response = new HttpResponseMessage(statusCode) { Content = new StringContent(responseBody) };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        }

        if (request.Headers.TryGetValues("X-Request-Id", out var values))
        {
            response.Headers.TryAddWithoutValidation("X-Request-Id", values.FirstOrDefault());
        }

        return response;
    }
}
