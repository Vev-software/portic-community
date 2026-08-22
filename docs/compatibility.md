# Compatibility statement

## API surface

- `POST /v1/messages` — request/response are the normalized `ChatRequest` / `ChatCompletion` contracts.
  The `/v1` prefix is the stability boundary: within `v1`, changes are **additive** (new optional
  fields, new providers). Breaking changes ship under a new version prefix (`/v2`).
- `GET /health` — stable, unversioned; shape `{"status":"ok"}`.

### `v1` request/response (current)

- Request: `model` (string, required), `messages` (array of `{role, content}`, ≥1 required),
  `maxTokens` (int, optional), `provider` (string, optional).
- Response: `id`, `model`, `provider`, `message {role, content}`, `usage {inputTokens, outputTokens,
  totalTokens}`.
- Errors: `400` `ProblemDetails` with a `title` reason code (`messages_required`,
  `provider_not_found`).

Clients should ignore unknown fields to remain forward-compatible.

## Provider SPI

`IChatProvider` + the normalized contracts are the adapter contract. New providers implement it without
any change to the core (see the fitness test). The SPI is expected to move to the Apache-2.0
`portic-sdk` package (ADR-0001); when it does, the **type shapes are preserved** — only the declaring
assembly/namespace changes. Adapters compiled against the placeholder should expect a one-time
namespace migration.

## Platform

- Runtime: .NET 10 (`net10.0`), SDK pinned via `global.json` (`10.0.100`, roll-forward to newer 10.0.x
  features). No dependency on OS-specific APIs. Bumped from .NET 9 to consume the published
  `Vev.Fabric.Contracts` package, which targets `net10.0` only.
- Dependencies: `Microsoft.Extensions.*` and ASP.NET 10.x (MIT). No AGPL-incompatible dependency is
  present (see ADR-0003 for the open `LICENSE`-file question).

## Stability level

**Pre-1.0 / preview.** The `v1` HTTP shape above is the intended stable surface, but until a tagged
release the contracts may still change. The vendor-neutral boundary (no provider SDK outside an
adapter) is a permanent, fitness-enforced invariant and will not be relaxed.
