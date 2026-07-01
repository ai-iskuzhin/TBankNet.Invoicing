# T-API — Выставление счетов (Invoicing)

Справочник по методам T-API T-Bank «Выставление счетов», на которые опирается `TBankNet.Invoicing`.
Официальная документация: <https://developer.tbank.ru/docs/products/invoicing>.

- **Базовый адрес (Production):** `https://business.tbank.ru/openapi/`
- **Базовый адрес (Sandbox):** `https://business.tbank.ru/openapi/sandbox/`
- **Авторизация:** `Authorization: Bearer <API_TOKEN>`
- **Трассировка:** заголовок `X-Request-Id` (UUID).
- **Scopes:** `H2H`, `Partner`

## Тело ошибки (`TBankInvoiceError`)

Для статусов вне диапазона 2xx (`400`, `401`, `403`, `422`, `429`, `500`).

| Поле | Тип | Обяз. |
| --- | --- | --- |
| `errorCode` | string ≤ 50 | да |
| `errorId` | string ≤ 50 | да |
| `errorMessage` | string ≤ 400 | да |
| `errorDetails` | object | нет |

## `POST /api/v1/invoice/send` — выставить счет

RPS: 4. Клиент: `SendInvoiceAsync`.

**Тело запроса:**

| Поле | Тип | Обяз. | Ограничения |
| --- | --- | --- | --- |
| `invoiceNumber` | string | да | `^\d{1,15}$` |
| `dueDate` | date | нет | `yyyy-MM-dd`, не меньше `invoiceDate` |
| `invoiceDate` | date | нет | `yyyy-MM-dd`; если не задана — текущая дата |
| `accountNumber` | string | нет | `^(\d{20}\|\d{22})$`; иначе главный счет компании |
| `payer` | object | нет | `name`, `inn`, `kpp` |
| `items` | array | нет | ≤ 100; `name`, `price`, `unit`, `vat`, `amount` |
| `contacts` | array | нет | ≤ 10; `email` |
| `contactPhone` | string | нет | `^((\+7)([0-9]){10})$` |
| `comment` | string | нет | ≤ 1000 символов |
| `customPaymentPurpose` | string | нет | ≤ 512 символов |

Ставка НДС позиции (`vat`) — строка: `None` (без НДС) или процент `[0, 5, 7, 10, 20, 22]`.

**Ответ `200`:**

| Поле | Тип | Обяз. |
| --- | --- | --- |
| `pdfUrl` | url | да (действует 10 дней) |
| `invoiceId` | uuid | да |
| `incomingInvoiceUrl` | url | нет (оплата через личный кабинет Т-Бизнеса) |

## `GET /api/v1/openapi/invoice/{invoiceId}/info` — статус счета

RPS: 20. Клиент: `GetInvoiceAsync`.

> Внимание: путь содержит дополнительный сегмент `openapi` (в отличие от `send`).

**Ответ `200`:**

| Поле | Тип | Значения |
| --- | --- | --- |
| `status` | string | `DRAFT` (черновик), `SUBMITTED` (отправлен), `EXECUTED` (оплачен) |
