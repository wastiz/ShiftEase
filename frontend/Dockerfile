# 1. Билд-стейдж
FROM node:22.21.1-alpine AS build

WORKDIR /app

COPY package.json package-lock.json ./

RUN npm install --legacy-peer-deps

COPY . .

ARG NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL

RUN npm run build

FROM node:22.21.1-alpine AS production

WORKDIR /app

COPY --from=build /app/node_modules ./node_modules
COPY --from=build /app/.next ./.next
COPY --from=build /app/public ./public
COPY --from=build /app/package.json ./package.json

EXPOSE 3000

CMD ["npm", "run", "start"]
