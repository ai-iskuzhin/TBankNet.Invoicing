using System.Text.Json;
using System.Text.Json.Serialization;

namespace TBankNet.Invoicing;

/// <summary>
/// Конвертер статуса счета: <c>DRAFT</c>, <c>SUBMITTED</c>, <c>EXECUTED</c>.
/// </summary>
internal sealed class TBankInvoiceStatusJsonConverter : JsonConverter<TBankInvoiceStatus>
{
    public override TBankInvoiceStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        return value switch
        {
            "DRAFT" => TBankInvoiceStatus.Draft,
            "SUBMITTED" => TBankInvoiceStatus.Submitted,
            "EXECUTED" => TBankInvoiceStatus.Executed,
            _ => throw TBankInvoiceWireParsing.UnknownEnumValue("invoice status", value)
        };
    }

    public override void Write(Utf8JsonWriter writer, TBankInvoiceStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(TBankInvoiceWireNames.FormatStatus(value));
    }
}
