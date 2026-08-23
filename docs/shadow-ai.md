# Portic as the sanctioned alternative to shadow AI

Shadow AI is a governance gap, not a demand problem. Employees adopt AI faster than most organizations
approve, route and observe it. Proofpoint's *The Definitive Guide to Shadow AI* says 78% of U.S.
workers who use AI at work use tools not provided by their employer, and Proofpoint's *State of AI
Security 2025* says 49% of organizations expect a shadow-AI incident within the next 12 months.
Those numbers describe the buyer pressure Portic is built for: teams want AI, but security and
engineering need a sanctioned path instead of blind exfiltration into consumer tools.

Sources:

- Proofpoint, *The Definitive Guide to Shadow AI*:
  https://www.proofpoint.com/us/resources/white-papers/definitive-guide-shadow-ai
- Proofpoint, *The State of AI Security 2025*:
  https://www.proofpoint.com/us/resources/threat-reports/state-ai-security-2025

## What Portic is

Portic is a self-hostable AI gateway and control point. It gives teams one governed path for model
traffic that *goes through Portic*: model access, routing, policy, audit signals, token visibility,
PII-redaction hooks, and provider abstraction.

That is the product promise:

- Replace private ChatGPT tabs and ad hoc API keys with a sanctioned gateway your team can actually use.
- Keep model traffic on infrastructure you control, including self-hosted and EU-resident deployment options.
- Preserve portability by keeping providers behind a vendor-neutral SPI instead of baking one vendor into the app.

## What Portic is not

Portic does **not** discover shadow AI on employee endpoints, browsers, or the wider network.
It does not claim CASB, endpoint agent, or browser-inspection coverage. It governs the AI traffic
that is intentionally routed through the gateway. That boundary is deliberate: Portic is the
sanctioned alternative to shadow AI, not a detector for every unsanctioned AI action in the estate.

## Why a sanctioned path matters

Blanket bans do not remove demand. A usable approved path does more:

- **Sovereignty:** self-host the gateway, choose where traffic runs, and keep provider choice open.
- **No-telemetry posture by default:** prompt/completion content is not emitted in telemetry or audit events.
- **Auditability:** gateway calls emit structured audit events and telemetry tags for provider/model/token visibility today, with richer identity tied to later Fabric integration.
- **PII-aware design:** the redaction port exists before content logging features land, so future views must redact first.
- **Spend control:** model allowlists, routing discipline, and token accounting create a base for governing cost instead of hiding it.

## Honest current-state boundary

Portic Community already proves the core gateway shape: normalized contracts, provider-neutral routing,
stub and package-based adapters, governance hooks, token usage, and content-free audit/telemetry.
Some controls are intentionally still minimal in Community:

- identity is currently a single-tenant placeholder, not full enterprise auth
- quotas are in-process, not a durable shared cost ledger
- audit storage is host-bound, not a full control-plane evidence system

That is still enough to demonstrate the essential product point: traffic that flows through Portic is
more governable than traffic that bypasses it.

## Scope boundary

Portic's core scope is model access, routing, governance, policy, observability, spend control and
AI audit. It is **not** trying to become a complete AI platform. Agent runtimes, RAG, document
processing and MCP registry work stay out of core and on separate tracks.
