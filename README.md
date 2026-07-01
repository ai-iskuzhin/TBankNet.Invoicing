<table>
  <tr>
    <td width="170" align="center" valign="middle">
      <img src="https://raw.githubusercontent.com/ai-iskuzhin/TBankNet.Invoicing/main/assets/icon.png" width="140" alt="Логотип TBankNet.Invoicing" />
    </td>
    <td valign="middle">
      <h1>TBankNet.Invoicing</h1>
      <p>Неофициальный .NET SDK для методов <a href="https://developer.tbank.ru/docs/products/invoicing">T-API T-Bank «Выставление счетов»</a>: выставление счетов на оплату (PDF + QR СБП) и отслеживание их статуса.</p>
      <p>
        <a href="https://github.com/ai-iskuzhin/TBankNet.Invoicing/blob/main/LICENSE"><img src="https://img.shields.io/github/license/ai-iskuzhin/TBankNet.Invoicing?style=flat-square" alt="License" /></a>
        <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/targets-netstandard2.0%20%7C%20net8.0%20%7C%20net10.0-512BD4?logo=dotnet&amp;style=flat-square" alt="Targets" /></a>
        <a href="https://www.nuget.org/packages/TBankNet.Invoicing"><img src="https://img.shields.io/nuget/v/TBankNet.Invoicing?logo=nuget&amp;style=flat-square" alt="Версия NuGet" /></a>
      </p>
    </td>
  </tr>
</table>

[English](README.en.md) · **Русский**

> Проект не аффилирован с T-Bank и разрабатывается независимо. «T-Bank» и связанные названия принадлежат их правообладателям.

## Установка

```bash
dotnet add package TBankNet.Invoicing
```

Поддерживаются `netstandard2.0`, `net8.0` и `net10.0`. На `net8.0`/`net10.0` используется встроенный `System.Text.Json`; на `netstandard2.0` подтягиваются `System.Text.Json` и `System.Net.Http.Json`.

## Поддерживаемые методы

| Метод T-API | API клиента | HTTP | RPS |
| --- | --- | --- | --- |
| Выставить счет | `SendInvoiceAsync` | `POST /api/v1/invoice/send` | 4 |
| Получить информацию о счете | `GetInvoiceAsync` | `GET /api/v1/openapi/invoice/{invoiceId}/info` | 20 |

Полное описание полей — в [docs/api-invoicing.md](docs/api-invoicing.md).

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
    ContactPhone = "+74996051110",
    Comment = "Информация для контрагента.",
});

Console.WriteLine(result.PdfUrl);           // ссылка на PDF (действительна 10 дней)
Console.WriteLine(result.InvoiceId);
Console.WriteLine(result.IncomingInvoiceUrl); // оплата через личный кабинет Т-Бизнеса
```

### Статус счета

```csharp
var info = await client.GetInvoiceAsync(result.InvoiceId);

Console.WriteLine(info.Status);   // Draft / Submitted / Executed
```

### Авторизация и трассировка

Авторизация — Bearer API-токен. Для каждого запроса генерируется заголовок `X-Request-Id`; его можно задать явно параметром `requestId`, а фактическое значение доступно в `result.Metadata.RequestId`.

## Обработка ошибок

Ответы со статусом вне диапазона 2xx приводят к `TBankInvoiceApiException` с разобранным телом ошибки (`errorCode`, `errorId`, `errorMessage`):

```csharp
try
{
    var result = await client.SendInvoiceAsync(request);
}
catch (TBankInvoiceValidationException ex)
{
    // Локальная валидация не пройдена (пустой номер счета, неверный телефон, срок оплаты раньше даты счета).
}
catch (TBankInvoiceApiException ex)
{
    Console.WriteLine($"HTTP {(int)ex.HttpStatusCode}: {ex.ErrorCode} — {ex.Error?.ErrorMessage}");
}
```

| Исключение | Когда возникает |
| --- | --- |
| `TBankInvoiceValidationException` | Локальная валидация запроса до отправки. |
| `TBankInvoiceApiException` | Ответ T-API со статусом вне диапазона 2xx. |
| `TBankInvoiceTransportException` | Ответ не получен (сеть, DNS, TLS). |
| `TBankInvoiceProtocolException` | Ответ получен, но не разбирается как ожидаемая модель. |

Все исключения наследуются от `TBankInvoiceException`.

## Лицензия

[MIT](LICENSE).
