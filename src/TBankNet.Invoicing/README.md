# TBankNet.Invoicing

Неофициальный .NET SDK для методов T-API T-Bank «Выставление счетов» (Invoicing).
Поддерживает `netstandard2.0`, `net8.0` и `net10.0`.

> Проект не аффилирован с T-Bank. «T-Bank» и связанные названия принадлежат их правообладателям.

## Установка

```bash
dotnet add package TBankNet.Invoicing
```

## Поддерживаемые методы

| Метод T-API | API клиента | HTTP |
| --- | --- | --- |
| Выставить счет | `SendInvoiceAsync` | `POST /api/v1/invoice/send` |
| Получить информацию о счете | `GetInvoiceAsync` | `GET /api/v1/openapi/invoice/{invoiceId}/info` |

## Быстрый старт

```csharp
using TBankNet.Invoicing;

using var httpClient = new HttpClient();

var client = new TBankInvoiceClient(httpClient, new TBankInvoiceClientOptions
{
    ApiToken = "YOUR_API_TOKEN",
    Environment = TBankInvoiceEnvironment.Sandbox   // или Production
});

var result = await client.SendInvoiceAsync(new TBankSendInvoiceRequest
{
    InvoiceNumber = "12345",
    DueDate = new DateTime(2026, 8, 22),
    Payer = new TBankInvoicePayer { Name = "ООО «Контрагент»", Inn = "730990470834", Kpp = "123456789" },
    Items = new[]
    {
        new TBankInvoiceItem { Name = "Рога", Price = 10m, Unit = "Шт", Vat = TBankInvoiceVat.None, Amount = 10m },
        new TBankInvoiceItem { Name = "Копыта", Price = 100m, Unit = "Шт", Vat = TBankInvoiceVat.Vat22, Amount = 2m },
    },
    Contacts = new[] { new TBankInvoiceContact { Email = "example@mail.com" } },
    Comment = "Информация для контрагента.",
});

Console.WriteLine(result.PdfUrl);      // ссылка на PDF (10 дней)
Console.WriteLine(result.InvoiceId);

var info = await client.GetInvoiceAsync(result.InvoiceId);
Console.WriteLine(info.Status);        // Draft / Submitted / Executed
```

## Обработка ошибок

- `TBankInvoiceValidationException` — локальная валидация запроса.
- `TBankInvoiceApiException` — ошибка API T-Bank (статус вне 2xx) с телом `errorCode` / `errorId` / `errorMessage`.
- `TBankInvoiceTransportException` — ответ не получен (сеть, DNS, TLS).
- `TBankInvoiceProtocolException` — ответ получен, но не разбирается как ожидаемая модель.

## Репозиторий

https://github.com/ai-iskuzhin/TBankNet.Invoicing
