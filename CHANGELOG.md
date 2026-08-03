# Changelog

All notable changes to this project are documented here. Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This project uses [Conventional Commits](https://www.conventionalcommits.org/).

## [Unreleased]

### Added

- First vertical slice: `POST /v1/messages` routes a normalized request through the provider **SPI**
  (`IChatProvider`) to the local **stub** (echo) adapter and returns a normalized completion.
- Ports-and-adapters layout: `Portic.Core` (contracts, SPI, router, orchestration, observability
  ports), `Portic.Providers.Stub` (echo adapter), `Portic.Gateway` (ASP.NET Core host).
- **Fitness harness** (`Portic.Architecture.Tests`) enforcing "no AI-provider SDK outside a provider
  adapter" at both the package-reference and compiled-assembly level.
- Content-free audit (`IAuditSink` + `LoggingAuditSink`) and `ActivitySource` telemetry.
- Scaffolding: `global.json` (pinned .NET 9), `Directory.Build.props` (analyzers + warnings-as-errors),
  `.editorconfig`, CI workflow (build + test + lint + fitness), devcontainer.
- Container packaging: multi-stage `Dockerfile` (chiseled, non-root, `:8080`), `.dockerignore`, and a
  `publish-image` workflow that pushes to `ghcr.io/vev-software/portic` on `main`/`v*` (ADR-0004).
- Docs: ADR-0001 (SPI → `portic-sdk`), ADR-0002 (audit/telemetry are Fabric contracts), ADR-0003
  (LICENSE-file discrepancy), ADR-0004 (container packaging is a Fabric concern), ops doc,
  threat-model note, compatibility statement.

### Changed

- Root `LICENSE` set to **GNU AGPL-3.0** (verbatim FSF text), matching AGENTS.md. Resolves the earlier
  Apache-2.0/AGPL-3.0 mismatch (ADR-0003).
