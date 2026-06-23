# Build the static bundle, then serve it with nginx.
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
# npm install (not `npm ci`) so platform-specific deps resolve for the build
# image — the committed lock file is generated on Windows and omits the
# linux-musl variants Alpine needs.
RUN npm install
COPY . .
# Baked into the bundle at build time — the browser uses this to reach the API.
ARG VITE_API_URL=http://localhost:5030
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

FROM nginx:alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
