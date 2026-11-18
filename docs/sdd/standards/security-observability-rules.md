# Security and observability rules

## Security and privacy

Apply least privilege. The host fallback policy requires authentication; explicitly mark intentionally public endpoints with `AllowAnonymous`. Preserve existing role and policy rules. See [Identity and access control](../architecture/security/README.md) for model details.

Do not log tokens, passwords, connection strings, personal data, or sensitive payloads. Do not return internal exception details in production responses.

## Package vulnerabilities

- CI (`verify` job in `.github/workflows/deploy-cloudrun.yml`) runs `dotnet list package --vulnerable --include-transitive` after restore; a High or Critical advisory in any direct or transitive NuGet package fails the build and so blocks the deploy. Fix it by updating the package (or pinning the transitive one in `Directory.Packages.props`), never by skipping the step.
- Dependabot (`.github/dependabot.yml`) opens one grouped pull request a month for NuGet minor and patch updates and one for GitHub Actions; security updates arrive on their own. They go through the same CI and review as any change.

## Logging and diagnostics

Use structured Serilog messages with stable, useful properties. Preserve correlation, cancellation, and error diagnostics. Choose suitable log levels without adding noise.

Handle transient failures explicitly and observably. Do not add hidden write retries that could duplicate side effects.

## Incoming webhooks

Applies to every endpoint that an outside system calls on its own (payment or delivery provider, Telegram, bank). Check it when you add or change one; the matching architect check is `T6 Incoming webhook`.

- **Authentication first.** The endpoint is anonymous for users, so the sender's proof is the only gate. Verify a signature (HMAC over the raw body) or the shared secret header with a constant-time comparison (`CryptographicOperations.FixedTimeEquals`) before any work; a missing or wrong proof answers 401 and nothing is read from the body. The secret lives in the secret store, one per environment, and is never logged.
- **Replay.** When the sender signs a timestamp, reject a request older than about five minutes. When it only repeats a secret header (Telegram), the dedupe key below is the replay protection.
- **Duplicates and order.** Events arrive at least once and in any order. Store the sender's event id under a unique index, or check the current state of the target row before acting (see [idempotency rules](idempotency-rules.md)); an already handled event answers 2xx so the sender stops retrying. A handler never assumes that event B came after event A.
- **Fast answer.** Accept, save, answer 2xx; slow work goes to the outbox, not into the request.
- **Money.** Before marking anything paid, compare the amount and status with the provider (a call to its API or a statement line), not only with the webhook body.
- **Reconciliation.** For a sender that can lose events, a scheduled job asks the sender for what happened since the last check and applies it through the same handler.
- **Logs.** Log the fact of each call (event id, time, result of the proof check, outcome) and never the secret, the signature, or personal data from the payload.
- **Tests.** A test for a wrong or missing proof (401), for the same event twice (one effect), and for events in reverse order.

KedrStore has no incoming webhook endpoint today; this section is the rule for the first one.
