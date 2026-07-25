# KedrStore documentation

This directory is the source of truth for architecture and specification-driven development (SDD).

- [Architecture](architecture/README.md) — actual backend topology and module boundaries.
- [Standards](standards/README.md) — mandatory implementation and documentation rules.
- [Operations](operations/README.md) — runtime configuration, startup, diagnostics and jobs.
- [Specifications](specs/README.md) — one folder per planned feature, plus reusable templates.
- [Legacy ADR archive](../legacy/adr/) — historical decisions. ADRs remain useful context, but new feature work starts from the SDD documents above.

Do not create one large design document. Keep a stable rule in architecture or standards; keep feature-specific decisions in that feature's specification.
