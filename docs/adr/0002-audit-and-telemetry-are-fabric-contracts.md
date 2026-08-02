# ADR-0002: Audit & telemetry bind to Fabric contracts; Portic ships only ports

- Status: **Proposed**
- Date: 2026-08-02

## Context

AGENTS.md forbids re-implementing a Fabric concern: "Identity, tenancy, RBAC, entitlement, **audit,
telemetry**, config, packaging → Fabric. If you need one and it's missing, propose the Fabric contract;
do not build a local copy." The Definition of Done (`15 §6`) nonetheless requires this slice to emit
**telemetry** and an **audit event**. These pull in opposite directions only if we build a local audit
*system*; they are compatible if Portic depends on a **port** and Fabric provides the implementation.

## Decision (proposed)

Portic defines thin ports and standard emission points, not pipelines:

- **Telemetry:** emit standard `System.Diagnostics.ActivitySource` spans from `PorticTelemetry`
  (`ActivitySourceName = "Portic.Gateway"`). A host binds this to OpenTelemetry / Fabric telemetry.
  Portic does not own exporters, sampling, or a collector.
- **Audit:** `IAuditSink` + a content-free `AuditEvent`. The community edition ships **only**
  `LoggingAuditSink` (writes structured metadata via `ILogger`, no persistence, no tamper-evidence).
  It is explicitly a placeholder.

**Proposed Fabric contract:** a `Fabric.Audit` sink accepting `{ eventType, timestamp, tenant,
principal, provider, model, outcome, reasonCode, tokensIn, tokensOut }` — content-free by construction
— which Portic binds to in place of `LoggingAuditSink`. Tenant/principal come from Fabric identity
context, not invented here.

## Consequences

- No local audit store or telemetry pipeline grows inside Portic (guardrail honored).
- `AuditEvent` has **no field capable of holding prompt/completion content**, so auditability can
  never become a content-exfiltration path (AGENTS.md E4/E5).
- When the Fabric contract lands, swap the sink registration in `AddPorticCore`; call sites are
  unchanged.
