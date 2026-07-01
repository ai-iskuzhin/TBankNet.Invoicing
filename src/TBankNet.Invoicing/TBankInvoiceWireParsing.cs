using System.Text.Json;

namespace TBankNet.Invoicing;

/// <summary>
/// Вспомогательные методы разбора проводных значений T-API.
/// </summary>
internal static class TBankInvoiceWireParsing
{
    /// <summary>
    /// Создает <see cref="JsonException"/> для неизвестного проводного значения enum.
    /// </summary>
    public static JsonException UnknownEnumValue(string kind, string? value)
    {
        return new JsonException(
            $"Unknown T-Bank {kind} value: '{value ?? "<null>"}'. This may be a new value not yet supported by " +
            "this version of TBankNet.Invoicing — please report it so it can be added: " +
            "https://github.com/ai-iskuzhin/TBankNet.Invoicing/issues/new");
    }
}
