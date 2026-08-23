# Ops: Portic Gateway

Operational notes for running the community gateway (the `POST /v1/messages` slice).

## What it is

A stateless ASP.NET Core service. No database, no external calls in the default (stub) configuration.
Horizontally scalable: run N replicas behind any HTTP load balancer; there is no shared state and the
request path never blocks on a control-plane store (AGENTS.md `04 §5`).

That boundary is enforced by `tests/Portic.Architecture.Tests`: runtime projects must not reference
database/control-plane client packages or direct DB APIs. Community quota and recent-call state are
bounded in-process views, not shared control-plane storage.

## Run

```bash
dotnet run --project src/Portic.Gateway          # dev, binds http://localhost:5187
dotnet publish src/Portic.Gateway -c Release      # produce a deployable build
```

Container/host should set `ASPNETCORE_URLS` (e.g. `http://0.0.0.0:8080`) to control the bind address.

### Container image

Images are published to GHCR by `.github/workflows/publish-image.yml` (on pushes to `main` and `v*`
tags): `ghcr.io/vev-software/portic-community` (`:latest`, `:main`, `:sha-…`, and `:X.Y.Z` on tags). The image
is chiseled and runs **non-root**, binding `:8080` by default. See ADR-0004 for the packaging→Fabric
boundary (signing/SBOM/promotion are deferred to a Fabric packaging contract).

```bash
docker run --rm -p 8080:8080 ghcr.io/vev-software/portic-community:latest
curl -X POST http://localhost:8080/v1/messages \
  -H 'Content-Type: application/json' \
  -d '{"model":"stub-echo","messages":[{"role":"user","content":"hello"}]}'
```

To build locally: `docker build -t portic-gateway . && docker run --rm -p 8080:8080 portic-gateway`.

## Configuration

| Setting | Env var | Default | Notes |
| --- | --- | --- | --- |
| Default provider | `Portic__DefaultProvider` | `stub` | Provider used when a request omits `provider`. |
| Recent-call store capacity | n/a | `256` rows | In-process ring buffer for `GET /v1/audit/recent-calls`; oldest rows are evicted first. |
| Bind URL | `ASPNETCORE_URLS` | `http://localhost:5187` (dev) | Standard ASP.NET binding. |
| Log level | `Logging__LogLevel__Default` | `Information` | Standard `Microsoft.Extensions.Logging`. |

**Secrets:** provider credentials are read from environment variables **inside the relevant provider
adapter only**. The stub needs none. Never place secrets in `appsettings.json` or in any config object
that is logged.

## Endpoints

- `POST /v1/messages` — normalized request → normalized completion. `400` with a reason-coded
  `ProblemDetails` for empty `messages` (`messages_required`) or an unknown `provider`
  (`provider_not_found`).
- `GET /v1/audit/recent-calls` — lists recent traffic that went through Portic itself, backed by the
  bounded in-process read model. Optional query filters: `provider`, `model`, `outcome`, `since`,
  `until`.
- `GET /health` — liveness/readiness probe, returns `{"status":"ok"}`. Wire this to your orchestrator.

### Recent-call inspection

Use the recent-call endpoint to inspect traffic that already flowed through the gateway, without raw log
access:

```bash
curl "http://localhost:5187/v1/audit/recent-calls?provider=stub&outcome=success"
```

Response rows include timestamp, route, provider, model, outcome, latency, token counts, content-state
flags, and cost-estimate metadata. Community limitations are explicit:

- retention is in-process and non-durable
- capacity is bounded and oldest rows are evicted first
- tenant/principal identity is still the Community placeholder unless the host composes a different request context
- the endpoint only reflects traffic through Portic; it does not discover usage outside the gateway

## Observability

- **Telemetry:** spans from `ActivitySource` **"Portic.Gateway"** (span `ai.message`, tags
  `portic.provider`, `portic.model`, `portic.tokens.input/output`). Subscribe with an OpenTelemetry
  listener/exporter in your host. See ADR-0002.
- **Audit:** structured `audit ai.message.completed|failed` log records (EventId 1000) with
  route/provider/model/outcome/reason/token counts, latency, tenant/principal ids, placeholder-vs-external
  identity state, explicit request/response content-state flags, and cost-estimate metadata
  (`Estimated`, `UnknownPricing`, or `NotComputed`) — **no message content**.
- **Recent-call read view:** Community also projects those same audit events into a bounded, in-process
  recent-call store for internal querying by provider, model, outcome, and time window. This is
  intentionally non-durable and eviction-based, suitable for a Community read view but not a control-plane
  database.
- **Cost semantics:** any monetary figure in Community audit metadata is an **estimate**, not invoice
  truth. Estimates must later be reconciled against provider invoices; unknown pricing is emitted
  explicitly instead of as an implied zero.
- **What is never emitted:** prompt or completion text, and any secret. Verified by design
  (`AuditEvent` has no content field) and by the threat model.
- **Community identity boundary:** Community currently emits the fixed single-tenant placeholder
  identity (`portic-community-default` / `anonymous`) unless a different `IRequestContextAccessor`
  is composed in by the host. Richer real-user identity remains a later Fabric integration concern.

## Failure modes

| Symptom | Likely cause | Action |
| --- | --- | --- |
| `400 provider_not_found` | Request `provider` has no registered adapter | Check spelling / that the adapter is registered at the composition root. |
| `400 messages_required` | Empty `messages` array | Client bug; send ≥1 message. |
| Startup fails binding options | Invalid `Portic` config section | `ValidateOnStart` surfaces it at boot — check env vars. |

## Rollback

Stateless: redeploy the previous image/build. No migrations, no data to reconcile.
