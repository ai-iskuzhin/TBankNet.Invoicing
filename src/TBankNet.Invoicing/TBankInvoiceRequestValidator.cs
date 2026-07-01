using System.Text.RegularExpressions;

namespace TBankNet.Invoicing;

/// <summary>
/// Консервативная локальная валидация запросов до отправки в T-API.
/// </summary>
/// <remarks>
/// Проверяет только очевидные ограничения из документации, чтобы поймать ошибки раньше сервера.
/// </remarks>
internal static class TBankInvoiceRequestValidator
{
    private const int CommentMaxLength = 1000;
    private const int PurposeMaxLength = 512;
    private const int MaxItems = 100;
    private const int MaxContacts = 10;

    // Use explicit [0-9] rather than \d: in .NET \d matches any Unicode decimal digit (Nd), but the
    // server rule is ASCII-only, so \d would let non-ASCII "digits" pass locally and fail server-side.
    private static readonly Regex InvoiceNumberRegex = new(@"^[0-9]{1,15}$", RegexOptions.CultureInvariant);
    private static readonly Regex AccountNumberRegex = new(@"^([0-9]{20}|[0-9]{22})$", RegexOptions.CultureInvariant);
    private static readonly Regex PhoneRegex = new(@"^((\+7)([0-9]){10})$", RegexOptions.CultureInvariant);

    public static void Validate(TBankSendInvoiceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.InvoiceNumber) || !InvoiceNumberRegex.IsMatch(request.InvoiceNumber))
        {
            throw new TBankInvoiceValidationException(
                $"{nameof(request.InvoiceNumber)} must be 1 to 15 digits.");
        }

        if (request.AccountNumber is { } account && !AccountNumberRegex.IsMatch(account))
        {
            throw new TBankInvoiceValidationException(
                $"{nameof(request.AccountNumber)} must contain 20 or 22 digits.");
        }

        if (request.ContactPhone is { } phone && !PhoneRegex.IsMatch(phone))
        {
            throw new TBankInvoiceValidationException(
                $"{nameof(request.ContactPhone)} must match +7XXXXXXXXXX.");
        }

        if (request.Comment is { Length: > CommentMaxLength })
        {
            throw new TBankInvoiceValidationException(
                $"{nameof(request.Comment)} must be at most {CommentMaxLength} characters.");
        }

        if (request.CustomPaymentPurpose is { Length: > PurposeMaxLength })
        {
            throw new TBankInvoiceValidationException(
                $"{nameof(request.CustomPaymentPurpose)} must be at most {PurposeMaxLength} characters.");
        }

        if (request.Items is { Count: > MaxItems })
        {
            throw new TBankInvoiceValidationException($"{nameof(request.Items)} must contain at most {MaxItems} items.");
        }

        if (request.Contacts is { Count: > MaxContacts })
        {
            throw new TBankInvoiceValidationException($"{nameof(request.Contacts)} must contain at most {MaxContacts} contacts.");
        }

        if (request.DueDate is { } due && request.InvoiceDate is { } issued && due.Date < issued.Date)
        {
            throw new TBankInvoiceValidationException(
                $"{nameof(request.DueDate)} must not be earlier than {nameof(request.InvoiceDate)}.");
        }

        if (request.Items is not null)
        {
            foreach (var item in request.Items)
            {
                if (item.Vat is { } vat && !Enum.IsDefined(typeof(TBankInvoiceVat), vat))
                {
                    throw new TBankInvoiceValidationException(
                        $"{nameof(TBankInvoiceItem.Vat)} must be None or one of 0, 5, 7, 10, 20, 22.");
                }
            }
        }
    }

    public static void ValidateInvoiceId(string invoiceId)
    {
        if (string.IsNullOrWhiteSpace(invoiceId))
        {
            throw new TBankInvoiceValidationException("invoiceId is required.");
        }
    }
}
