# ADR 0045: Upgrade to Tailwind CSS v3

## Status
Accepted

## Date
2025-06-25

## Context
Tailwind CSS v4 has been used in the project for styling. However, it has been observed that downgrading to Tailwind CSS v3 offers several advantages:

Alternatives considered:
- Continue using Tailwind CSS v4
- Downgrade to Tailwind CSS v3

Tailwind CSS v3 was selected because it:

- Improves developer experience with better tooling and features.
- Has enhanced support and active maintenance.
- Reduces issues with styling and compatibility.

Tailwind CSS v4 was not selected because it was found to be unstable and caused issues during implementation.

## Decision
Upgrade the project to use Tailwind CSS v3.

## Consequences

### Positive
- Access to the latest features and improvements.
- Better support and active development.
- Simplified handling of styles.
- Easier integration and support for Shuffle.dev.

### Negative
- Migration effort required for compatibility.
- Updates needed for documentation and configuration files.
