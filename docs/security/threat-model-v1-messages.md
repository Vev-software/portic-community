# Threat model note: `POST /v1/messages` (first slice)

Scope: the one route shipped in this slice and its path through the SPI to the stub adapter. Lightweight
STRIDE-style pass per the Definition of Done; expand as real provider adapters and entitlement gating land.

## Assets

- **Customer content** (prompts/completions) in flight.
- **Provider credentials** (none for stub; real for future adapters).
- **Integrity of the vendor-neutral boundary** (the "never vendor-bound" guardrail itself is an asset).

## Trust boundaries

Client → Gateway (HTTP) → core → provider adapter → (future) external provider API. The stub keeps the
last hop in-process, so this slice has no outbound network trust boundary yet.

## Findings

| # | Category | Threat | Mitigation (this slice) | Follow-up |
| --- | --- | --- | --- | --- |
| 1 | Information disclosure | Prompt/response content leaks via logs/telemetry/audit | `AuditEvent` has **no content field**; telemetry tags carry only provider/model/token counts; completion logs carry no content. Verified in the run log during dev. | Add a redaction test asserting no content in emitted logs. |
| 2 | Information disclosure | Provider secret leaks via config object or telemetry | Secrets read from env **inside the adapter only**; not modeled in `GatewayOptions`; never logged. | Secret-handling review per real adapter. |
| 3 | Tampering | A change quietly calls a provider SDK outside an adapter, coupling the runtime to a vendor | **Fitness test** fails the build if a non-adapter project/assembly references a provider SDK. | Keep `ProviderSdkPolicy` markers current as SDKs appear. |
| 4 | Denial of service | Unbounded request body / message count exhausts memory | ASP.NET default request-size limits apply; stub does no heavy work. | Add explicit body-size + message-count limits and rate limiting before real providers. |
| 5 | Spoofing / EoP | Unauthenticated caller uses the gateway; no tenant/principal | **Out of scope for this slice by design** — identity/authn/entitlement are Fabric concerns (AGENTS.md). Not stubbed locally. | Bind Fabric identity + entitlement (`09`) before any non-stub exposure. Reason codes already threaded (`provider_not_found`). |
| 6 | Repudiation | No durable audit trail | `LoggingAuditSink` emits content-free audit events; not durable/tamper-evident. | Bind Fabric audit contract (ADR-0002). |

## Residual risk

This slice is safe to run locally against the stub. It is **not** production-exposed: it has no authn,
tenancy, entitlement, rate limiting, or durable audit — all intentionally deferred to Fabric contracts
rather than reimplemented here. Do not expose it publicly until findings 4–6 are addressed.
