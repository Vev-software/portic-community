# AGENTS.md — `portic`

Portic is VEV's **AI gateway & control plane**: one governed, portable gateway for every model.
This is the PUBLIC community repo (runtime). Read this file fully before making changes. It is
system-prompt-level guidance; the constraints below are non-negotiable.

> Same file is read by Claude Code and Codex/other agents. `CLAUDE.md` points here — keep this the
> single source of truth.

## Stack & versions

- Language / runtime: **.NET 10 / C# 14** (SDK pinned in `global.json` to `10.0.100`, `rollForward: latestFeature`)
- Framework(s): **ASP.NET Core Minimal APIs** (host); **xUnit** (tests)
- Package manager: **NuGet** (`dotnet` CLI; package versions pinned in each `.csproj`)
- Datastore(s): **none** in core today. The request path must never block on a control-plane DB
  (`04 §5`); any future control-plane store stays off the request path.

## Commands

Run from the repo root. `Portic.sln` ties the projects together.

- Build:  `dotnet build Portic.sln -c Release`   (analyzers + `TreatWarningsAsErrors` gate the build)
- Test:   `dotnet test Portic.sln -c Release`   (unit + integration + fitness)
- Lint:   `dotnet format Portic.sln`   (verify-only in CI: `dotnet format Portic.sln --verify-no-changes`)
- Run (local, ten-minute goal): `dotnet run --project src/Portic.Gateway`
  → then `curl -X POST http://localhost:5187/v1/messages -H 'Content-Type: application/json' -d '{"model":"stub-echo","messages":[{"role":"user","content":"hello"}]}'`
  succeeds against the local **stub** provider (no API key). See `README.md`.
- Fitness/architecture tests: `dotnet test tests/Portic.Architecture.Tests` — enforces the guardrails
  below (SDK-boundary). Never disable them; fix the code, not the test.

## Non-negotiable guardrails (fitness-tested — see `15 §2`)

- **AI-native, never vendor-bound.** Every model call goes through the provider SPI / AI contract
  (`10`). **No direct AI-provider SDK calls anywhere outside a provider adapter.** The abstraction is
  permanent; providers are disposable.
- **No product depends on another product's code.** Cross-product is events + published APIs + shared
  Fabric context only (`04 §6`), never another product's internals.
- **Never re-implement a Fabric concern.** Identity, tenancy, RBAC, entitlement, audit, telemetry,
  config, packaging → Fabric. If you need one and it's missing, **propose the Fabric contract; do not
  build a local copy** (`15 §2`).
- **No `if (plan == "…")`.** Ask the entitlement service (`09`). Fail-static on security / budget /
  licensing.
- **Control plane ≠ data plane.** The request path never blocks on the control-plane DB (`04 §5`).
- **No secrets in telemetry; no customer content logged by default** (`15 §2` E4/E5).
- **API/SDK first.** The UI orchestrates the API; it is never the only path.
- **Community edition is production-worthy, not crippled.** Commercial value lives in
  `portic-enterprise`, gated by entitlements — never by degrading the free core.

## Scope — what is OUT of core (do not add to this repo)

Per `15 §4`: **agent runtime, RAG, document processing, and the MCP registry are out of core** —
separate integration tracks. Do not pull them into the gateway core. Core = routing, provider
governance, AI cost/policy, prompt libraries, AI audit.

## Licensing

- This runtime: **AGPL-3.0**. Client SDKs / provider SPI / contracts: **Apache-2.0** (`portic-sdk`).
- Do not relicense. Do not add a dependency whose licence is incompatible with AGPL-3.0 for the
  runtime, or with Apache-2.0 for SDK/contract code. Flag any uncertain dependency instead of adding.

## Working rules

- **Verify every change:** run build + test + lint + fitness before finishing. A change that makes a
  fitness test fail is wrong — fix the change, do not weaken the test.
- **Contract-first:** if a change touches a boundary, update or add the contract/schema first, then
  the implementation, then tests.
- **Definition of done** (`15 §6`): domain contract, API, SDK, CLI (where relevant), ops docs,
  telemetry, audit events, entitlement gating with reason codes, tests (unit + integration +
  architecture), a threat-model note, a compatibility statement.
- **When unsure, read — don't guess.** The handbook section is cited inline (e.g. `10`, `09`).
  If a needed Fabric contract doesn't exist, open an ADR proposing it rather than inventing one.
- Keep PRs small and single-purpose. Conventional commits. Update this file in the same PR when a
  convention changes.

## Where to read (index)

- AI contract / provider SPI → handbook `10`, repo `portic-sdk`
- Entitlements → `09`   ·   Control plane vs data plane → `04 §5`, `06`
- Repository & dependency rules (CI-enforced) → `02 §7`
- Product obligations & hard rules → `15`   ·   Roadmap / first slices → `13`

## Do NOT touch

- Licensing headers, `LICENSE`, `NOTICE`, CLA files.
- CI dependency-rule / fitness workflows, except to strengthen them.
- Anything under `portic-enterprise` (separate private repo) — it must never be a build dependency of
  this community repo (`02 §1.4`).
