# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-10-02

### Added

- `TBankInvoiceClient.GetInvoiceDocumentAsync` — downloads the invoice file from the
  `PdfUrl` returned by `SendInvoiceAsync`, returning `TBankInvoiceDocument`
  (bytes, `Content-Type`, and the file name the bank serves).

  Previously the SDK handed back a URL and left retrieval to the caller, which meant
  every consumer wrote its own `HttpClient` call — and a plain one does not work:
  `business.tbank.ru` is issued by the Минцифры "Russian Trusted" CA, which is absent
  from most container trust stores, so the handshake fails with `UntrustedRoot`. Going
  through the client means the download inherits whatever handler the `HttpClient` was
  configured with, so it cannot silently miss that trust.

  The request is sent **without** the Bearer token: the document link carries its own
  token, and the URL arrives from the bank's response via caller code, so the API token
  must not travel there. The link's host is checked against the configured environment,
  so the method cannot be used to fetch arbitrary URLs, and the served file name is
  stripped of any path before being returned.

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
