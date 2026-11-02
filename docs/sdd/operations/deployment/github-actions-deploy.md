# GitHub Actions deploy to Cloud Run

[`.github/workflows/deploy-cloudrun.yml`](../../../../.github/workflows/deploy-cloudrun.yml) runs `verify` (build, unit, architecture and integration tests) on every pull request and push. On a push to `master` the `deploy` job then builds the `Host.Api` and `Host.Jobs` images, pushes them to Artifact Registry, runs the `database-migrate` job, deploys the `api` service and updates the Cloud Run jobs. Project, region and service names are `env` values at the top of the workflow.

## Google Cloud sign-in

The deploy job signs in without a key, through Workload Identity Federation: GitHub issues a short-lived OIDC token, pool `github` / provider `github-oidc` in project `classkedrproject` accepts it only for repository `Weretik/KedrStore`, and the job acts as `gha-deployer@classkedrproject.iam.gserviceaccount.com`. No service account key or `GCP_SA_KEY` secret is used. The service account keeps its existing deploy roles. The repository principal set has `roles/iam.workloadIdentityUser` on the project (the binding on the service account itself was refused), so it may impersonate service accounts of `classkedrproject`; only `Weretik/KedrStore` can obtain it.

If sign-in fails at "Auth to Google Cloud", check the provider's attribute condition (repository name) and that project binding before anything else.
