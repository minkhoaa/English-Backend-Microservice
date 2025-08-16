# syntax=docker/dockerfile:1.7


FROM node:24-alpine AS build
WORKDIR /app

COPY package*.json ./

RUN --mount=type=cache,target=/root/.npm \
    npm ci

COPY . .

RUN npm run build \
 && npm prune --omit=dev


FROM node:24-alpine AS runtime
WORKDIR /app

# ENV tối ưu runtime
ENV NODE_ENV=production \
    NODE_OPTIONS=--enable-source-maps \
    NPM_CONFIG_LOGLEVEL=error


COPY --from=build /app/package*.json ./
COPY --from=build /app/node_modules ./node_modules
COPY --from=build /app/dist ./dist


COPY --from=build /app/src/mail/templates ./src/mail/templates

# Bảo mật: chạy user không phải root
RUN addgroup -S app && adduser -S app -G app
USER app

EXPOSE 3000

# (Tùy chọn) Healthcheck đơn giản
# HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
#   CMD node -e "fetch('http://127.0.0.1:3000/health').then(r=>{if(r.ok)process.exit(0);process.exit(1)}).catch(()=>process.exit(1))"

CMD ["node", "dist/main.js"]
