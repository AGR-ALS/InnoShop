FROM node:22.17-alpine AS builder

WORKDIR /app

COPY inno-shop/package.json inno-shop/package-lock.json ./
RUN npm ci

COPY inno-shop ./

RUN npm run build

FROM node:22.17-alpine AS runner

WORKDIR /app
ENV NODE_ENV=production

COPY --from=builder /app/package.json ./package.json
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/.next ./.next
COPY --from=builder /app/public ./public
COPY --from=builder /app/next.config.ts ./next.config.ts

EXPOSE 3000

CMD ["npm", "run", "start"]
