# ADR-0010: Keyless deploy from GitHub Actions to Cloud Run

- Status: accepted
- Date: 2026-11-15 (recorded from the current workflow)
- Owner: Виталий (project owner)

## Context

The backend runs on Google Cloud Run in `europe-central2`. A long-lived service account key stored in GitHub could be leaked and used to read the production database and secrets. Releases must not depend on a manual step on the owner's computer.

## Alternatives considered

- A JSON service account key in a GitHub secret: rejected; long-lived and easy to leak.
- Cloud Build triggers: possible, but the tests and images already build in GitHub Actions.
- Deploy by hand from a laptop: rejected; not repeatable.

## Decision

- Every merge to `main` runs `.github/workflows/deploy-cloudrun.yml`: build and test, build the API and job images, push them to Artifact Registry.
- GitHub signs in to Google Cloud through Workload Identity Federation (pool `github`, provider `github-oidc`), accepted only for `Weretik/KedrStore`; the job acts as the deployer service account. No key exists.
- Secrets (database, 1C, Telegram, CDN, proxy secret) live in Secret Manager and reach Cloud Run as references; none is in the repository.
- The migration job runs first; the API is deployed only if it succeeds ([ADR-0002](0002-one-shot-jobs-on-cloud-run.md)).

## Why

- Nothing long-lived to steal.
- Every release is the same steps, visible in the Actions log.

## Trade-offs

- The Workload Identity setup lives in Google Cloud and must be recreated by hand for a new project.
- A failed deploy needs the Actions log and Cloud Run console to diagnose.

## Sources and checks

- [GitHub Actions deploy](../../operations/deployment/github-actions-deploy.md), [configuration and secrets](../../operations/configuration/configuration-and-secrets.md).
