# Cloudflare API proxy

The public API address is `api.classkedr.com.ua`. A Cloudflare Worker serves that host and forwards every request to the Cloud Run service. Cloud Run domain mapping is not available in the service region, and a Google load balancer costs a fixed monthly fee, so the Worker is the chosen entry point.

```text
Browser / mobile app
  | https://api.classkedr.com.ua/...
  v
Cloudflare Worker (deploy/cloudflare/api-proxy)
  | adds X-Kedr-Client-IP + X-Kedr-Proxy-Secret
  v
Cloud Run Host.Api (*.run.app)
  | TrustedProxyClientIpMiddleware restores the client IP
  v
rate limiting, auth, audit logs
```

## Why the API needs this host

The API and the frontends must be on the same site (`classkedr.com.ua`):

- The refresh cookie becomes first-party, so browsers that block third-party cookies keep it.
- The CSRF cookie is set with `Domain=classkedr.com.ua` (`Identity:SessionCookies:CsrfCookieDomain` in `appsettings.Production.json`), so the admin and storefront can read it and send it in `X-CSRF-Token` on refresh. A cookie set by `*.run.app` is never readable by a `classkedr.com.ua` page.

## Client IP behind the proxy

Cloud Run sees the Worker's egress IP, not the user's. The Worker sends the original `CF-Connecting-IP` in `X-Kedr-Client-IP`, together with a shared secret in `X-Kedr-Proxy-Secret`. `TrustedProxyClientIpMiddleware` replaces `RemoteIpAddress` only when the secret matches, and always removes both headers. A direct request to the Cloud Run URL therefore cannot spoof its IP.

| Setting | Where | Secret |
| --- | --- | --- |
| `TrustedProxy:SharedSecret` | Cloud Run, from Secret Manager secret `api-proxy-shared-secret` (see the deploy workflow) | Yes |
| `PROXY_SHARED_SECRET` | Worker secret, same value | Yes |
| `ORIGIN_URL` | Worker variable, the Cloud Run service URL | No |
| `TrustedProxy:ClientIpHeaderName`, `SecretHeaderName` | `appsettings.json`; must match `worker.js` | No |

With an empty `TrustedProxy:SharedSecret` the middleware only strips the headers and changes nothing else.

## Setup order

1. Generate one random secret (for example `openssl rand -hex 32`).
2. Create Secret Manager secret `api-proxy-shared-secret` with it and grant the Cloud Run service account `Secret Manager Secret Accessor` on it. The deploy workflow fails if the secret is missing.
3. Deploy the Worker from `deploy/cloudflare/api-proxy`: `npx wrangler deploy`, then `npx wrangler secret put PROXY_SHARED_SECRET`. The `custom_domain` route creates the DNS record and certificate for `api.classkedr.com.ua`; the zone must be in the same Cloudflare account.
4. Check `https://api.classkedr.com.ua/health`.
5. Merge the backend change (deploys `CsrfCookieDomain` and the secret), then switch the frontends' production API URL to `https://api.classkedr.com.ua`.

## Limits

- The Workers free plan allows 100,000 requests per day.
- The Cloud Run URL stays publicly reachable. Do not rely on the proxy for access control; endpoints must enforce their own authorization.
- Local frontends (`localhost`) calling the deployed API are cross-site: login works, but the CSRF cookie is not readable there, so refresh works only against a local API.
