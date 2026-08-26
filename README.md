# Portic · Community Edition

Portic is VEV's **AI gateway & control plane**: one governed, portable gateway for every model.
This repository is the free, self-hostable **community runtime** (`portic-community`, AGPL-3.0);
the paid modules, hosted management and commercial integrations live in
**[`portic-enterprise`](https://github.com/Vev-software/portic-enterprise)** (private) and
compose onto this runtime through Fabric entitlements. Agent/contributor guardrails live in
[`AGENTS.md`](./AGENTS.md) — read it before changing anything.

The gateway is built ports-and-adapters style: every model call goes through a provider **SPI**, and
each provider is a disposable **adapter**. A local **stub** adapter ships in-box so a clean clone
runs with no API key.

```
HTTP  ──►  Portic.Gateway  ──►  IMessageGateway ──► IProviderRouter ──► IChatProvider (SPI)
(POST /v1/messages)          (Portic.Core, provider-neutral)                    │
                                                                                ▼
                                                                   Portic.Providers.Stub (echo)
```

## Ten-minute local run

Prerequisites: the **.NET 10 SDK** (pinned in `global.json`). Then, from the repo root:

```bash
dotnet run --project src/Portic.Gateway
```

The gateway listens on `http://localhost:5187`. In another terminal:

```bash
curl -X POST http://localhost:5187/v1/messages \
  -H 'Content-Type: application/json' \
  -d '{"model":"stub-echo","messages":[{"role":"user","content":"hello portic"}]}'
```

Response (served by the local stub — no API key, no network):

```json
{
  "id": "stub-…",
  "model": "stub-echo",
  "provider": "stub",
  "message": { "role": "assistant", "content": "echo: hello portic" },
  "usage": { "inputTokens": 2, "outputTokens": 3, "totalTokens": 5 }
}
```

`GET /health` returns `{"status":"ok"}`.

### Or run the container

Published to GHCR on every push to `main` (and on `v*` tags). Binds `:8080`, runs non-root:

```bash
docker run --rm -p 8080:8080 ghcr.io/vev-software/portic-community:latest
# then POST to http://localhost:8080/v1/messages as above
```

Build it yourself with `docker build -t portic-gateway .`. See
[ADR-0004](./docs/adr/0004-container-packaging-is-a-fabric-concern.md) for the packaging→Fabric boundary.

## Build, test, lint, fitness

```bash
dotnet build Portic.sln -c Release        # analyzers + warnings-as-errors gate the build
dotnet test  Portic.sln -c Release        # unit + integration + fitness
dotnet format Portic.sln                  # lint (auto-fix); CI runs --verify-no-changes
dotnet test tests/Portic.Architecture.Tests   # fitness/architecture guardrails only
```

## Layout

| Project | Role |
| --- | --- |
| `src/Portic.Core` | Domain: normalized contracts, the provider **SPI**, router, orchestration, observability ports. No provider SDK, no ASP.NET. |
| `src/Portic.Providers.Stub` | The local **echo** provider adapter. The one place provider SDKs are allowed to live. |
| `src/Portic.Gateway` | ASP.NET Core host mapping HTTP ⇄ contracts. |
| `tests/Portic.Core.Tests` | Unit tests. |
| `tests/Portic.Gateway.IntegrationTests` | Boots the host and drives `POST /v1/messages`. |
| `tests/Portic.Architecture.Tests` | **Fitness tests** — enforce the "no provider SDK outside an adapter" boundary. |

## Integrations

- [Atlas Community](./docs/integrations/atlas-community.md) — gateway contract, authentication model, and example requests

## Product docs

- [Portic as the sanctioned alternative to shadow AI](./docs/shadow-ai.md) — product framing, scope boundary, and sovereignty posture

## Guardrails

The headline guardrail — *AI-native, never vendor-bound* — is machine-enforced: a fitness test fails
if any non-adapter project or assembly references an AI-provider SDK. Adding a new provider means
adding a `Portic.Providers.<name>` adapter that implements `IChatProvider`; nothing in the core
changes. See [`AGENTS.md`](./AGENTS.md) and [`docs/`](./docs) (ADRs, ops, threat model, compatibility).

The data-plane boundary is machine-enforced too: runtime projects must not reference
database/control-plane client packages or call direct DB APIs. Durable control-plane state belongs
outside this Community runtime; local quota and recent-call state are bounded, in-process views.

The product scope boundary is also fitness-tested: agent runtime, RAG, document processing, and MCP
registry work must stay on separate integration tracks rather than growing inside the Community core.

## Configuration

- `Portic:DefaultProvider` (env `Portic__DefaultProvider`) — provider used when a request names none. Default `stub`.
- Provider **credentials are supplied via environment variables only**, read inside the relevant
  adapter. No secrets in config objects, logs, or telemetry; no customer content logged by default.

## Licensing

Runtime: **AGPL-3.0** (see [`LICENSE`](./LICENSE)). Client SDKs / provider SPI / contracts are
**Apache-2.0** in `portic-sdk` (see
[`docs/adr/0001-provider-spi-location.md`](./docs/adr/0001-provider-spi-location.md) and
[`docs/adr/0005-license-policy.md`](./docs/adr/0005-license-policy.md)).
[ADR-0003](./docs/adr/0003-license-file-discrepancy.md) records the resolution of an earlier
Apache-2.0/AGPL-3.0 mismatch in the `LICENSE` file.
