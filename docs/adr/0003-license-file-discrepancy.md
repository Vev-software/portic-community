# ADR-0003: `LICENSE` file is Apache-2.0 but the runtime must be AGPL-3.0

- Status: **Accepted — resolved via Option 1** (maintainer decision, 2026-08-03)
- Date: 2026-08-02

> **Resolution (2026-08-03):** the maintainer confirmed the runtime really is AGPL-3.0. The root
> `LICENSE` was replaced with the verbatim GNU AGPL-3.0 text (from `https://www.gnu.org/licenses/agpl-3.0.txt`),
> matching AGENTS.md "Licensing". Current dependencies (Microsoft.Extensions.* / ASP.NET 9.x, all MIT)
> are AGPL-compatible. The "Do NOT touch `LICENSE`" guardrail was overridden by explicit maintainer
> instruction for this one change.

## Context

AGENTS.md is unambiguous: **"This runtime: AGPL-3.0"** ("Licensing"), and the task framing calls this
the "PUBLIC community runtime, AGPL-3.0". However, the committed `LICENSE` file at the repo root is the
**Apache License 2.0** verbatim. AGENTS.md also lists `LICENSE` under **"Do NOT touch"** (alongside
`NOTICE`/CLA files).

So there is a direct conflict:

- The stated policy says the runtime is AGPL-3.0.
- The actual `LICENSE` file says Apache-2.0.
- The rules forbid an agent from editing `LICENSE`.

Apache-2.0 vs AGPL-3.0 is not a cosmetic difference: AGPL adds the network-use copyleft that is
typically the entire point of shipping a gateway runtime as "community, production-worthy, not
crippled". Relicensing is a legal act with CLA/contributor implications.

## Decision

**Deferred to the maintainers. This was left unchanged by design.** An agent must not pick a license.
Two coherent resolutions exist; a human must choose:

1. **The runtime really is AGPL-3.0** (matches AGENTS.md): replace the root `LICENSE` with the AGPL-3.0
   text and add a `NOTICE`. Confirm every current dependency is AGPL-compatible (they are: the
   Microsoft.Extensions.* and ASP.NET packages are MIT).
2. **The root `LICENSE` is intentionally Apache-2.0** (e.g. this tree also seeds `portic-sdk`): then
   AGENTS.md's "This runtime: AGPL-3.0" line is wrong or premature and must be corrected, and the
   AGPL-only source should move under a clearly AGPL-licensed path.

## Consequences / what was done instead

- No `LICENSE` edit was made (honoring "Do NOT touch").
- No AGPL-incompatible dependency was added, so either resolution stays open.
- README and this ADR flag the discrepancy so it is visible before any public release.
