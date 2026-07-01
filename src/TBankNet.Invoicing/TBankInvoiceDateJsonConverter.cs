using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TBankNet.Invoicing;

/// <summary>
/// Конвертер даты формата <c>yyyy-MM-dd</c> (<c>String&lt;date&gt;</c> в T-API) для <see cref="DateTime"/>.
/// </summary>
/// <remarks>
/// Используется точечно через атрибут на свойствах-датах, чтобы не затрагивать <see cref="DateTimeOffset"/>-поля.
/// <c>DateOnly</c> недоступен на netstandard2.0, поэтому используется <see cref="DateTime"/> (только дата).
/// </remarks>
internal sealed class TBankInvoiceDateJsonConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-dd";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallback))
        {
            return fallback.Date;
        }

        throw new JsonException($"Expected a date in '{Format}' format, got '{value ?? "<null>"}'.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
