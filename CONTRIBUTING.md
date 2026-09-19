# Contributing to Portic Community

Thanks for your interest in improving Portic. Please read
[`AGENTS.md`](./AGENTS.md) first — it is the single source of truth for build/test
commands, code conventions, and the non-negotiable architectural guardrails.

## Contributor License Agreement — required

Portic Community is **dual-licensed** (AGPL-3.0 **and** commercial — see
[`LICENSING.md`](./LICENSING.md)). For that to work, every contribution must be made under
terms that let VEV distribute it under **both** licenses. Before your pull request can be
merged you must agree to the [Contributor License Agreement](./CLA.md):

1. Sign off every commit with `git commit -s` (Developer Certificate of Origin).
2. In your first pull request, confirm you have read and agree to [`CLA.md`](./CLA.md).

You keep the copyright to your contribution — the CLA is a license grant, not an
assignment. We cannot merge contributions that are not covered by the CLA, because a single
AGPL-only contribution would break the project's ability to offer a commercial license.

## Before you open a PR

- Keep PRs **small and single-purpose**; use Conventional Commits.
- Run the full local gate:

  ```bash
  dotnet build Portic.sln -c Release        # analyzers + warnings-as-errors
  dotnet test  Portic.sln -c Release        # unit + integration + fitness
  dotnet format Portic.sln --verify-no-changes
  ```

- Do not weaken a fitness/architecture test to make a change pass — fix the change.
- Respect the guardrails in [`AGENTS.md`](./AGENTS.md): the community edition is
  **production-worthy, not crippled**; commercial value lives in `portic-enterprise`,
  gated by entitlements, never by degrading the free core.

## Reporting security issues

Do not open a public issue for a vulnerability. Follow the process in `SECURITY.md` if
present, or contact the maintainers privately.
