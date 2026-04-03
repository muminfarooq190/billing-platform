FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY tsconfig.json ./
COPY src ./src
RUN npm run build

FROM node:20-alpine AS runtime
RUN addgroup -S app && adduser -S app -G app
WORKDIR /app
COPY --from=build /app/package*.json ./
COPY --from=build /app/node_modules ./node_modules
COPY --from=build /app/dist ./dist
RUN chown -R app:app /app
USER app
EXPOSE 3000
HEALTHCHECK --interval=15s --timeout=5s --retries=5 CMD wget --spider -q http://localhost:3000/health || exit 1
ENTRYPOINT ["node", "dist/main.js"]
