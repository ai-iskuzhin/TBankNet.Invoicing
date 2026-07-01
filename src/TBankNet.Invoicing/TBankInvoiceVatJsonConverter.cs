using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TBankNet.Invoicing;

/// <summary>
/// Конвертер ставки НДС: на проводе строка (<c>"None"</c> или процент «22»), в модели — <see cref="TBankInvoiceVat"/>.
/// </summary>
internal sealed class TBankInvoiceVatJsonConverter : JsonConverter<TBankInvoiceVat>
{
    public override TBankInvoiceVat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // НДС приходит строкой ("None"/"22"), но допускаем и число на случай нестрогого сервера.
        var value = reader.TokenType == JsonTokenType.Number
            ? reader.GetInt32().ToString(CultureInfo.InvariantCulture)
            : reader.GetString();

        if (string.Equals(value, "None", StringComparison.OrdinalIgnoreCase))
        {
            return TBankInvoiceVat.None;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var percent)
            && Enum.IsDefined(typeof(TBankInvoiceVat), percent))
        {
            return (TBankInvoiceVat)percent;
        }

        throw TBankInvoiceWireParsing.UnknownEnumValue("invoice VAT rate", value);
    }

    public override void Write(Utf8JsonWriter writer, TBankInvoiceVat value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(TBankInvoiceWireNames.FormatVat(value));
    }
}
