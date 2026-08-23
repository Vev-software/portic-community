# Compatibility statement

## API surface

- `POST /v1/messages` — request/response are the normalized `ChatRequest` / `ChatCompletion` contracts.
  The `/v1` prefix is the stability boundary: within `v1`, changes are **additive** (new optional
  fields, new providers). Breaking changes ship under a new version prefix (`/v2`).
- `GET /v1/audit/recent-calls` — lists recent traffic that passed through Portic, with optional filters
  `provider`, `model`, `outcome`, `since`, and `until`. The shape is additive within `v1`; retention is
  in-process and non-durable by design in Community.
- `GET /health` — stable, unversioned; shape `{"status":"ok"}`.

### `v1` request/response (current)

- Request: `model` (string, required), `messages` (array of `{role, content}`, ≥1 required),
  `maxTokens` (int, optional), `provider` (string, optional).
- Response: `id`, `model`, `provider`, `message {role, content}`, `usage {inputTokens, outputTokens,
  totalTokens}`.
- Recent-call row: `timestamp`, `eventType`, `route`, `provider`, `model`, `outcome`, `latencyMs`,
  `tenantId`, `principalId`, `identityState`, `requestContentState`, `responseContentState`,
  `inputTokens`, `outputTokens`, `costEstimationStatus`, `estimatedCost`, `estimatedCostCurrency`,
  `costEstimationSource`, `reasonCode`.
- Errors: `ProblemDetails` with a `title` reason code: `400` (`messages_required`, `provider_not_found`),
  `403` (`model_not_allowed`), `429` (`quota_exceeded`).

Clients should ignore unknown fields to remain forward-compatible.

## Provider SPI

`IChatProvider` + the normalized contracts are the adapter contract. New providers implement it without
any change to the core (see the fitness test). The SPI and normalized contracts now come from the
Apache-2.0 `Portic.Sdk` package on nuget.org; the runtime no longer declares local duplicates in
`Portic.Core`. The **type shapes are preserved** — the change was assembly/namespace location, not
wire semantics or runtime behavior.

## Governance policy

`Portic.Core.Governance` enforces core, free-tier gateway governance ahead of routing:

- **Model allowlist** (`Portic:Policy:AllowedModels`) — empty (the default) permits every model,
  matching today's behavior. A configured, non-empty list is a strict allowlist; a request for a
  model not on it is denied `403 model_not_allowed`.
- **Per-team quota** (`Portic:Policy:TeamDailyQuotas`) — a team with no entry is unlimited. A team is
  resolved from the principal's `team` claim, falling back to the tenant when absent (today: the
  single-tenant placeholder). Exceeding the configured daily count denies `429 quota_exceeded`.
  Counting is in-process and resets on restart — a durable, shared quota store is hosted/enterprise
  scope, not this edition's.
- Both checks are **fail-safe**: an internal error in quota evaluation denies rather than silently
  passing the request through.

This is distinct from the entitlement seam below: governance/policy is core (free), not
entitlement-gated (`13-Portic-Roadmap.md`).

**PII redaction** (`IContentRedactor` / `RegexPiiRedactor`) ships as a tested, ready-to-consume port
— there is no current call site, because no current code path persists or displays prompt/completion
content (the audit event is content-free by design). It exists ahead of the planned usage/audit view
(portic-community#17), so that feature is required to redact before it can log anything.

## Entitlement seam

`Portic.Core.Entitlements` gates every paid capability through the Fabric entitlement contract
(`Vev.Fabric.Contracts`), never a hand-rolled plan check (fitness-tested). Community's evaluator
(`CommunityEntitlementService`) has an empty grant set and denies every reserved paid capability
(`PorticCapabilities.ReservedPaid`) unconditionally — there is no configuration, snapshot or remote
source that could change that. The free gateway core does not call the gate at all today, so this
has no effect on `POST /v1/messages` or any other current endpoint; it exists so a future paid
capability composes through entitlement from day one instead of migrating onto it later.

Tenant/principal identity is currently a fixed single-tenant placeholder
(`SingleTenantRequestContextAccessor`) — there is no authentication. Real Fabric identity is a
separate, later contract to consume; swapping it in is a DI registration change, not a call-site
change (mirrors the audit/telemetry placeholder pattern in ADR-0002).

## Platform

- Runtime: .NET 10 (`net10.0`), SDK pinned via `global.json` (`10.0.100`, roll-forward to newer 10.0.x
  features). No dependency on OS-specific APIs. Bumped from .NET 9 to consume the published
  `Vev.Fabric.Contracts` package, which targets `net10.0` only.
- Dependencies: `Microsoft.Extensions.*` and ASP.NET 10.x (MIT). No AGPL-incompatible dependency is
  present. ADR-0005 records the accepted runtime/SPI/enterprise license policy.

## Stability level

**Pre-1.0 / preview.** The `v1` HTTP shape above is the intended stable surface, but until a tagged
release the contracts may still change. The vendor-neutral boundary (no provider SDK outside an
adapter) is a permanent, fitness-enforced invariant and will not be relaxed.
