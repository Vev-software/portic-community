# ADR-0005: License policy for Portic Community, SDK contracts, and enterprise runtime

## Status

Accepted.

## Context

Portic intentionally splits the public gateway runtime from the provider-neutral contracts and SPI:

- `portic-community` is the self-hostable gateway/runtime repository.
- `portic-sdk` carries the normalized request/response contracts and provider SPI consumed by adapter authors.
- enterprise and hosted control-plane/runtime features are delivered outside this Community repository.

The license line must support those boundaries. Runtime source should not default to Apache-2.0, because
the Community gateway is the network-facing open runtime. At the same time, adapter and client authors
need permissively licensed contracts so they can integrate without taking a runtime copyleft dependency
just to compile against the SPI.

ADR-0001 moved the provider SPI and normalized contracts to the Apache-2.0 `Portic.Sdk` package.
ADR-0003 resolved the historical mismatch where this repository's root `LICENSE` had been Apache-2.0
while the stated runtime policy was AGPL-3.0.

## Decision

The Portic licensing policy is:

- `portic-community` gateway/runtime source is licensed under AGPL-3.0, matching the root `LICENSE`.
- `portic-sdk` contracts, client SDKs, and provider SPI are licensed under Apache-2.0.
- provider adapters may reference the Apache-2.0 SPI package; adapter-specific license choices happen
  in their own repositories/packages, subject to dependency compatibility.
- enterprise and hosted runtime/control-plane features are proprietary unless explicitly published under
  another license by the maintainers.
- this repository must not present the runtime as Apache-2.0 or BSL-1.1.

## Consequences

- Runtime contributors and downstream operators read the AGPL-3.0 obligations from the root `LICENSE`.
- Integrators can build clients and provider adapters against `Portic.Sdk` without depending on this
  AGPL runtime repository.
- Future packaging metadata for runtime artifacts must use `AGPL-3.0-only` or an explicitly approved
  equivalent expression.
- Future contract/SPI packages must stay in `portic-sdk` or another permissively licensed package, not
  reintroduced as the only public source inside the AGPL runtime.
- Any proposal to move Community runtime code to BSL or Apache requires a new ADR and maintainer approval.
