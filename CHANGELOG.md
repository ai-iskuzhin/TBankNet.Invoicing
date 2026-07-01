# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-07-01

### Added

- Initial release of `TBankNet.Invoicing`: an unofficial .NET SDK for the T-Bank
  T-API "Invoicing" (Выставление счетов) methods.
- `TBankInvoiceClient` with typed methods:
  - `SendInvoiceAsync` — `POST /api/v1/invoice/send`
  - `GetInvoiceAsync` — `GET /api/v1/openapi/invoice/{invoiceId}/info`
- Bearer-token authentication, automatic `X-Request-Id` generation (overridable
  per call), and `Production` / `Sandbox` environment selection.
- Typed enums (`TBankInvoiceStatus`, `TBankInvoiceVat`), response metadata, and a
  transport / protocol / validation / API exception hierarchy.
- Multi-targeting for `netstandard2.0`, `net8.0` and `net10.0`.
