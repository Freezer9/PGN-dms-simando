# Multi-stage Docker build for React SPA with Caddy reverse proxy

# Stage 1: Build React SPA bundle
FROM oven/bun:1-alpine AS builder
WORKDIR /app

# Copy dependency manifests first for layer caching
COPY package.json bun.lock tsconfig*.json tsr.config.json biome.json components.json index.html vite.config.ts ./
RUN bun install --frozen-lockfile

# Copy source code and build production assets
COPY src/ ./src
RUN bun run build

# Stage 2: Serve with Caddy web server
FROM caddy:2-alpine AS runtime
WORKDIR /srv

# Copy built frontend assets to web root
COPY --from=builder /app/dist /srv

# Copy custom Caddy configuration
COPY Caddyfile /etc/caddy/Caddyfile

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --spider -q http://localhost:80/ || exit 1

CMD ["caddy", "run", "--config", "/etc/caddy/Caddyfile", "--adapter", "caddyfile"]
