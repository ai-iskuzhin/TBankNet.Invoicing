<table>
  <tr>
    <td width="170" align="center" valign="middle">
      <img src="https://raw.githubusercontent.com/ai-iskuzhin/TBankNet.Invoicing/main/assets/icon.png" width="140" alt="TBankNet.Invoicing logo" />
    </td>
    <td valign="middle">
      <h1>TBankNet.Invoicing</h1>
      <p>Unofficial .NET SDK for the <a href="https://developer.tbank.ru/docs/products/invoicing">T-Bank T-API "Invoicing"</a> methods: issue payment invoices (PDF + SBP QR) to counterparties and track their payment status.</p>
      <p>
        <a href="https://github.com/ai-iskuzhin/TBankNet.Invoicing/blob/main/LICENSE"><img src="https://img.shields.io/github/license/ai-iskuzhin/TBankNet.Invoicing?style=flat-square" alt="License" /></a>
        <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/targets-netstandard2.0%20%7C%20net8.0%20%7C%20net10.0-512BD4?logo=dotnet&amp;style=flat-square" alt="Targets" /></a>
        <a href="https://www.nuget.org/packages/TBankNet.Invoicing"><img src="https://img.shields.io/nuget/v/TBankNet.Invoicing?logo=nuget&amp;style=flat-square" alt="NuGet version" /></a>
      </p>
    </td>
  </tr>
</table>

**English** · [Русский](README.md)

> This project is not affiliated with T-Bank and is developed independently. "T-Bank" and related names belong to their respective owners.

## Installation

```bash
dotnet add package TBankNet.Invoicing
```

Targets `netstandard2.0`, `net8.0` and `net10.0`. On `net8.0`/`net10.0` the built-in `System.Text.Json` is used; on `netstandard2.0` the `System.Text.Json` and `System.Net.Http.Json` packages are pulled in.

## Supported methods

| T-API method | Client API | HTTP | RPS |
| --- | --- | --- | --- |
| Send an invoice | `SendInvoiceAsync` | `POST /api/v1/invoice/send` | 4 |
| Get invoice info | `GetInvoiceAsync` | `GET /api/v1/openapi/invoice/{invoiceId}/info` | 20 |

Full field reference: [docs/api-invoicing.md](docs/api-invoicing.md).

## Quick start

```csharp
using TBankNet.Invoicing;

using var httpClient = new HttpClient();

var client = new TBankInvoiceClient(httpClient, new TBankInvoiceClientOptions
{
    ApiToken = "YOUR_API_TOKEN",
    Environment = TBankInvoiceEnvironment.Sandbox   // or Production
});

var result = await client.SendInvoiceAsync(new TBankSendInvoiceRequest
{
    InvoiceNumber = "12345",
    DueDate = new DateTime(2026, 8, 22),
    Payer = new TBankInvoicePayer { Name = "OOO Counterparty", Inn = "730990470834", Kpp = "123456789" },
    Items = new[]
    {
        new TBankInvoiceItem { Name = "Horns", Price = 10m, Unit = "pcs", Vat = TBankInvoiceVat.None, Amount = 10m },
        new TBankInvoiceItem { Name = "Hooves", Price = 100m, Unit = "pcs", Vat = TBankInvoiceVat.Vat22, Amount = 2m },
    },
    Contacts = new[] { new TBankInvoiceContact { Email = "example@mail.com" } },
    ContactPhone = "+74996051110",
    Comment = "Note for the counterparty.",
});

Console.WriteLine(result.PdfUrl);            // PDF link (valid for 10 days)
Console.WriteLine(result.InvoiceId);
Console.WriteLine(result.IncomingInvoiceUrl); // pay via the T-Business account

var info = await client.GetInvoiceAsync(result.InvoiceId);
Console.WriteLine(info.Status);              // Draft / Submitted / Executed
```

### Authentication and tracing

Authentication uses a Bearer API token. An `X-Request-Id` header is generated for every request; you can set it explicitly via the `requestId` parameter, and the effective value is available on `result.Metadata.RequestId`.

## Error handling

Responses with a status outside the 2xx range raise a `TBankInvoiceApiException` carrying the parsed error body (`errorCode`, `errorId`, `errorMessage`):

```csharp
try
{
    var result = await client.SendInvoiceAsync(request);
}
catch (TBankInvoiceValidationException ex)
{
    // Local validation failed (empty invoice number, invalid phone, due date before invoice date).
}
catch (TBankInvoiceApiException ex)
{
    Console.WriteLine($"HTTP {(int)ex.HttpStatusCode}: {ex.ErrorCode} — {ex.Error?.ErrorMessage}");
}
```

| Exception | When it is thrown |
| --- | --- |
| `TBankInvoiceValidationException` | Local request validation before sending. |
| `TBankInvoiceApiException` | T-API response with a status outside the 2xx range. |
| `TBankInvoiceTransportException` | No response received (network, DNS, TLS). |
| `TBankInvoiceProtocolException` | Response received but not parseable into the expected model. |

All exceptions derive from `TBankInvoiceException`.

## License

[MIT](LICENSE).
