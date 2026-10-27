# KedrStore documentation

Versioned KedrStore engineering documentation is stored here. Backend documents are maintained alongside code changes.

## Backend SDD

Start at the [backend SDD entry point](sdd/README.md): it maps each question to architecture, standards, operations, contracts, specifications, and the [feature map](sdd/features/README.md). Stable terms are in the [product glossary](product/glossary.md).

## Historical documentation

The [archive](legacy/README.md) keeps older ADRs, architecture notes, client PDFs, and feature materials from before the current SDD set. It provides context and sources for comparison; it does not automatically define rules for new work.

## Information placement

Stable system descriptions belong in `sdd/architecture`, and normative rules in `sdd/standards`. Current feature structure and code navigation belong in `sdd/features/`; decisions, requirements, contracts, tasks, and verification evidence belong in the corresponding `sdd/specs/` directory.
