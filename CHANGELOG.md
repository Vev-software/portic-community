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
- Scaffolding: `global.json` (pinned .NET 10), `Directory.Build.props` (analyzers + warnings-as-errors),
  `.editorconfig`, CI workflow (build + test + lint + fitness), devcontainer.
- Container packaging: multi-stage `Dockerfile` (chiseled, non-root, `:8080`), `.dockerignore`, and a
  `publish-image` workflow that pushes to `ghcr.io/vev-software/portic-community` on `main`/`v*` (ADR-0004).
- Docs: ADR-0001 (SPI → `portic-sdk`), ADR-0002 (audit/telemetry are Fabric contracts), ADR-0003
  (LICENSE-file discrepancy), ADR-0004 (container packaging is a Fabric concern), ops doc,
  threat-model note, compatibility statement.
- Docs: Atlas Community integration guide (`docs/integrations/atlas-community.md`) with gateway
  contract, authentication model, and example curl request. Closes #21.
- Fail-static entitlement seam (`Portic.Core.Entitlements`): `PaidCapabilityGate` (`Require`/
  `Evaluate`), the reserved paid capability taxonomy (`PorticCapabilities`), and Community's
  always-deny `CommunityEntitlementService`, consuming the Fabric entitlement contract
  (`Vev.Fabric.Contracts`). A `SingleTenantRequestContextAccessor` placeholder stands in for real
  Fabric identity. Fitness test bans `if (plan == …)` anywhere in `src/`. Closes #16.
- Granular core governance (`Portic.Core.Governance`): a configurable model allowlist and per-team
  daily quota enforced before routing (`GovernancePolicyGate`, `PolicyOptions`), fail-safe on
  internal error, denying `403 model_not_allowed` / `429 quota_exceeded`. `IContentRedactor` /
  `RegexPiiRedactor` ships as a tested, ready-to-consume PII-redaction port ahead of the planned
  usage/audit view (#17) — no current call site, since no current path logs content. Free-tier, not
  gated by the entitlement seam. Closes #18.

### Changed

- Bumped to **.NET 10** (from .NET 9) to consume `Vev.Fabric.Contracts`, which publishes a
  `net10.0`-only build.

- Root `LICENSE` set to **GNU AGPL-3.0** (verbatim FSF text), matching AGENTS.md. Resolves the earlier
  Apache-2.0/AGPL-3.0 mismatch (ADR-0003).
