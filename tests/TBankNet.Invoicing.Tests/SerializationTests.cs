using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TBankNet.Invoicing.Tests;

public sealed class SerializationTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    [Fact]
    public void SendRequest_serializes_dates_vat_and_cyrillic()
    {
        var request = new TBankSendInvoiceRequest
        {
            InvoiceNumber = "12345",
            DueDate = new DateTime(2020, 8, 22),
            InvoiceDate = new DateTime(2020, 7, 23),
            AccountNumber = "40802123456789012345",
            Payer = new TBankInvoicePayer { Name = "ООО «Ромашка»", Inn = "730990470834", Kpp = "123456789" },
            Items = new[]
            {
                new TBankInvoiceItem { Name = "Рога", Price = 10m, Unit = "Шт", Vat = TBankInvoiceVat.None, Amount = 10m },
                new TBankInvoiceItem { Name = "Копыта", Price = 100m, Unit = "Шт", Vat = TBankInvoiceVat.Vat22, Amount = 2m },
            },
        };

        var json = JsonSerializer.Serialize(request, Options);

        Assert.Contains("\"invoiceNumber\":\"12345\"", json);
        Assert.Contains("\"dueDate\":\"2020-08-22\"", json);      // date-only format
        Assert.Contains("\"invoiceDate\":\"2020-07-23\"", json);
        Assert.Contains("\"vat\":\"None\"", json);                // без НДС
        Assert.Contains("\"vat\":\"22\"", json);                  // процент строкой
        Assert.Contains("\"name\":\"Рога\"", json);               // кириллица не экранируется
        Assert.DoesNotContain("\\u04", json);
    }

    [Fact]
    public void SendRequest_omits_null_optionals()
    {
        var request = new TBankSendInvoiceRequest { InvoiceNumber = "1" };

        var json = JsonSerializer.Serialize(request, Options);

        Assert.DoesNotContain("dueDate", json);
        Assert.DoesNotContain("payer", json);
        Assert.DoesNotContain("items", json);
    }

    [Fact]
    public void SendResult_deserializes()
    {
        const string json = """
        {
          "pdfUrl": "https://example.com/qwetq",
          "invoiceId": "d8327c28-4a8e-4084-93ea-a94b7bd144c5",
          "incomingInvoiceUrl": "https://business.tbank.ru/sme/invoices/incoming/d8327c28-4a8e-4084-93ea-a94b7bd144c5"
        }
        """;

        var result = JsonSerializer.Deserialize<TBankInvoiceSendResult>(json, Options);

        Assert.Equal("d8327c28-4a8e-4084-93ea-a94b7bd144c5", result!.InvoiceId);
        Assert.Equal("https://example.com/qwetq", result.PdfUrl);
        Assert.NotNull(result.IncomingInvoiceUrl);
    }

    [Theory]
    [InlineData("DRAFT", TBankInvoiceStatus.Draft)]
    [InlineData("SUBMITTED", TBankInvoiceStatus.Submitted)]
    [InlineData("EXECUTED", TBankInvoiceStatus.Executed)]
    public void Info_deserializes_status(string wire, TBankInvoiceStatus expected)
    {
        var info = JsonSerializer.Deserialize<TBankInvoiceInfo>($$"""{ "status": "{{wire}}" }""", Options);

        Assert.Equal(expected, info!.Status);
    }

    [Fact]
    public void Unknown_status_throws()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<TBankInvoiceInfo>("""{ "status": "PAID" }""", Options));
    }

    [Fact]
    public void Vat_enum_value_equals_percent()
    {
        Assert.Equal(22, (int)TBankInvoiceVat.Vat22);
        Assert.Equal(0, (int)TBankInvoiceVat.Vat0);
    }
}
