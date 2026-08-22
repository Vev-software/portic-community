# ADR-0004: Container packaging is a Fabric concern; ship a minimal community image

- Status: **Accepted (interim)** — maintainer requested a community image now
- Date: 2026-08-03

## Context

AGENTS.md lists **packaging** among the Fabric concerns: "Identity, tenancy, RBAC, entitlement, audit,
telemetry, config, **packaging** → Fabric. If you need one and it's missing, propose the Fabric
contract; do not build a local copy." Standardized packaging/distribution across VEV products
(base images, signing/SBOM/provenance, tagging and promotion policy, registry layout) is therefore
expected to come from a Fabric packaging contract, not to be re-invented per product.

At the same time, the community edition must be "production-worthy, not crippled": a person cloning
this repo should be able to `docker run` the gateway without assembling packaging themselves.

## Decision

Ship a **minimal, standard** container image and GHCR publish pipeline in this repo as an interim,
and propose the Fabric packaging contract to supersede the policy pieces:

- `Dockerfile`: multi-stage, chiseled non-root ASP.NET 10 runtime, no app-specific packaging platform.
- `.github/workflows/publish-image.yml`: builds and pushes to `ghcr.io/<owner>/portic` on pushes to
  `main` and on `v*` tags, authenticating with the built-in `GITHUB_TOKEN` (no new secret).
- The image links back to the repo via `org.opencontainers.image.source` so provenance is discoverable.

**Proposed Fabric packaging contract** (to replace the policy, not the Dockerfile): base-image choice
and cadence, image signing (cosign) + SBOM + build provenance/attestation, tag/promotion policy
(dev → staging → release), and registry naming/visibility. When it lands, this repo consumes it and
drops any duplicated policy here.

## Consequences

- **Now:** a clean clone yields a runnable image; publication uses only `GITHUB_TOKEN`.
- **Deliberately NOT built here** (belongs to Fabric): signing, SBOM/attestation, multi-registry
  promotion, org-wide base-image governance. Their absence is a known gap, tracked by this ADR — do
  not paper over it with a bespoke local implementation.
- The image is **unauthenticated and stub-only** just like the runtime today; do not expose it
  publicly until the threat-model follow-ups (authn/tenancy/rate-limit/durable audit) are addressed
  (see docs/security/threat-model-v1-messages.md).
