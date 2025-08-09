# ===== BUILD STAGE =====
FROM node:24-alpine AS build
WORKDIR /app

# 1) Copy file khai báo để tận dụng cache
COPY package*.json ./
RUN npm ci

# 2) Copy code
COPY . .

# 3) Build & lược bỏ dev deps
RUN npm run build && npm prune --omit=dev

# ===== RUNTIME STAGE =====
FROM node:24-alpine
WORKDIR /app
ENV NODE_ENV=production

# 4) Copy node_modules production & dist vào image gọn nhẹ
COPY --from=build /app/node_modules ./node_modules
COPY --from=build /app/package*.json ./
COPY --from=build /app/dist ./dist
COPY --from=build /app/src/mail/templates ./src/mail/templates  
# (Nếu bạn có assets như template email ở src, xem mục “Copy assets” bên dưới)
# Ví dụ:
# COPY --from=build /app/dist/mail/templates ./dist/mail/templates

EXPOSE 3000
CMD ["node", "dist/main.js"]
