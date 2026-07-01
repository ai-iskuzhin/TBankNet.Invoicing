using System.Net;

namespace TBankNet.Invoicing.Tests;

public sealed class ClientTests
{
    private const string SendResponse = """
    {
      "pdfUrl": "https://example.com/qwetq",
      "invoiceId": "d8327c28-4a8e-4084-93ea-a94b7bd144c5",
      "incomingInvoiceUrl": "https://business.tbank.ru/sme/invoices/incoming/d8327c28-4a8e-4084-93ea-a94b7bd144c5"
    }
    """;

    private static TBankInvoiceClient CreateClient(
        StubHttpMessageHandler handler,
        TBankInvoiceEnvironment environment = TBankInvoiceEnvironment.Production)
    {
        var options = new TBankInvoiceClientOptions { ApiToken = "secret-token", Environment = environment };
        return new TBankInvoiceClient(new HttpClient(handler), options);
    }

    [Fact]
    public async Task SendInvoice_posts_to_send_path_with_bearer_and_request_id()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, SendResponse);
        var client = CreateClient(handler);

        var result = await client.SendInvoiceAsync(new TBankSendInvoiceRequest
        {
            InvoiceNumber = "12345",
            Items = new[] { new TBankInvoiceItem { Name = "Рога", Price = 10m, Amount = 1m } },
        });

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("https://business.tbank.ru/openapi/api/v1/invoice/send", handler.LastRequest.RequestUri!.AbsoluteUri);
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal("secret-token", handler.LastRequest.Headers.Authorization.Parameter);
        Assert.True(handler.LastRequest.Headers.Contains("X-Request-Id"));
        Assert.Contains("\"invoiceNumber\":\"12345\"", handler.LastRequestBody);
        Assert.Equal("d8327c28-4a8e-4084-93ea-a94b7bd144c5", result.InvoiceId);
        Assert.Equal(HttpStatusCode.OK, result.Metadata!.HttpStatusCode);
    }

    [Fact]
    public async Task GetInvoice_builds_info_path_with_double_openapi_segment()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, """{ "status": "SUBMITTED" }""");
        var client = CreateClient(handler, TBankInvoiceEnvironment.Sandbox);

        var info = await client.GetInvoiceAsync("d8327c28-4a8e-4084-93ea-a94b7bd144c5");

        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.Equal(
            "https://business.tbank.ru/openapi/sandbox/api/v1/openapi/invoice/d8327c28-4a8e-4084-93ea-a94b7bd144c5/info",
            handler.LastRequest.RequestUri!.AbsoluteUri);
        Assert.Equal(TBankInvoiceStatus.Submitted, info.Status);
    }

    [Fact]
    public async Task Error_status_throws_api_exception_with_parsed_body()
    {
        const string errorBody = """
        { "errorCode": "INVALID_INVOICE", "errorId": "abc-1", "errorMessage": "invoiceNumber is invalid" }
        """;
        var handler = new StubHttpMessageHandler(HttpStatusCode.UnprocessableEntity, errorBody);
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<TBankInvoiceApiException>(() =>
            client.GetInvoiceAsync("d8327c28-4a8e-4084-93ea-a94b7bd144c5"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.HttpStatusCode);
        Assert.Equal("INVALID_INVOICE", exception.ErrorCode);
        Assert.Equal("invoiceNumber is invalid", exception.Error!.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abcdef")]                 // не цифры
    [InlineData("1234567890123456")]        // 16 цифр
    [InlineData("١٢٣")]                     // арабо-индийские цифры — ASCII-only правило
    public async Task SendInvoice_rejects_bad_invoice_number(string number)
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, SendResponse);
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<TBankInvoiceValidationException>(() =>
            client.SendInvoiceAsync(new TBankSendInvoiceRequest { InvoiceNumber = number }));
    }

    [Fact]
    public async Task SendInvoice_rejects_due_date_before_invoice_date()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, SendResponse);
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<TBankInvoiceValidationException>(() =>
            client.SendInvoiceAsync(new TBankSendInvoiceRequest
            {
                InvoiceNumber = "1",
                InvoiceDate = new DateTime(2026, 8, 22),
                DueDate = new DateTime(2026, 8, 21),
            }));
    }

    [Fact]
    public async Task SendInvoice_rejects_bad_phone()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, SendResponse);
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<TBankInvoiceValidationException>(() =>
            client.SendInvoiceAsync(new TBankSendInvoiceRequest { InvoiceNumber = "1", ContactPhone = "89990001122" }));
    }

    [Fact]
    public async Task GetInvoice_rejects_empty_id()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, """{ "status": "DRAFT" }""");
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<TBankInvoiceValidationException>(() => client.GetInvoiceAsync("  "));
    }
}
