// Cloudflare Worker that serves the KedrStore API on api.classkedr.com.ua.
// It forwards every request to the Cloud Run service and passes the response back unchanged,
// so the API shares the classkedr.com.ua site with the frontends (first-party cookies).
//
// Configuration (Workers > Settings > Variables and Secrets):
//   ORIGIN_URL           plain text, the Cloud Run URL, e.g. https://api-449981321140.europe-central2.run.app
//   PROXY_SHARED_SECRET  secret, same value as the Cloud Run secret api-proxy-shared-secret

const CLIENT_IP_HEADER = 'X-Kedr-Client-IP';
const SECRET_HEADER = 'X-Kedr-Proxy-Secret';

export default {
  async fetch(request, env) {
    const incomingUrl = new URL(request.url);
    const targetUrl = new URL(incomingUrl.pathname + incomingUrl.search, env.ORIGIN_URL);

    const proxied = new Request(targetUrl, request);
    proxied.headers.delete('Host');
    proxied.headers.delete(CLIENT_IP_HEADER);
    proxied.headers.delete(SECRET_HEADER);

    const clientIp = request.headers.get('CF-Connecting-IP');
    if (clientIp && env.PROXY_SHARED_SECRET) {
      proxied.headers.set(CLIENT_IP_HEADER, clientIp);
      proxied.headers.set(SECRET_HEADER, env.PROXY_SHARED_SECRET);
    }

    proxied.headers.set('X-Forwarded-Host', incomingUrl.host);
    proxied.headers.set('X-Forwarded-Proto', 'https');

    return fetch(proxied, { redirect: 'manual' });
  },
};
