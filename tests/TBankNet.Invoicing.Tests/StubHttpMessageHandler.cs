using System.Net;

namespace TBankNet.Invoicing.Tests;

/// <summary>
/// Тестовый обработчик HTTP: возвращает заранее заданный ответ и сохраняет последний запрос.
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode statusCode;
    private readonly string responseBody;

    public StubHttpMessageHandler(HttpStatusCode statusCode, string responseBody)
    {
        this.statusCode = statusCode;
        this.responseBody = responseBody;
    }

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

        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseBody),
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        if (request.Headers.TryGetValues("X-Request-Id", out var values))
        {
            response.Headers.TryAddWithoutValidation("X-Request-Id", values.FirstOrDefault());
        }

        return response;
    }
}
