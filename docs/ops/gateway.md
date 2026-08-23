# Ops: Portic Gateway

Operational notes for running the community gateway (the `POST /v1/messages` slice).

## What it is

A stateless ASP.NET Core service. No database, no external calls in the default (stub) configuration.
Horizontally scalable: run N replicas behind any HTTP load balancer; there is no shared state and the
request path never blocks on a control-plane store (AGENTS.md `04 §5`).

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
| Bind URL | `ASPNETCORE_URLS` | `http://localhost:5187` (dev) | Standard ASP.NET binding. |
| Log level | `Logging__LogLevel__Default` | `Information` | Standard `Microsoft.Extensions.Logging`. |

**Secrets:** provider credentials are read from environment variables **inside the relevant provider
adapter only**. The stub needs none. Never place secrets in `appsettings.json` or in any config object
that is logged.

## Endpoints

- `POST /v1/messages` — normalized request → normalized completion. `400` with a reason-coded
  `ProblemDetails` for empty `messages` (`messages_required`) or an unknown `provider`
  (`provider_not_found`).
- `GET /health` — liveness/readiness probe, returns `{"status":"ok"}`. Wire this to your orchestrator.

## Observability

- **Telemetry:** spans from `ActivitySource` **"Portic.Gateway"** (span `ai.message`, tags
  `portic.provider`, `portic.model`, `portic.tokens.input/output`). Subscribe with an OpenTelemetry
  listener/exporter in your host. See ADR-0002.
- **Audit:** structured `audit ai.message.completed|failed` log records (EventId 1000) with
  route/provider/model/outcome/reason/token counts, latency, tenant/principal ids, placeholder-vs-external
  identity state, and explicit request/response content-state flags — **no message content**.
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
