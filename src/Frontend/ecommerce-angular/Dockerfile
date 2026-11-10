# Multi-stage Dockerfile for Angular 19 Frontend
FROM node:20-alpine AS build
WORKDIR /app

# Cache dependencies
COPY package*.json ./
RUN npm ci

# Copy source and build production bundle
COPY . .
RUN npm run build -- --configuration production

# Production Nginx runtime
FROM nginx:1.27-alpine AS final
WORKDIR /usr/share/nginx/html

# Replace default Nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy compiled static assets from build stage
COPY --from=build /app/dist/ecommerce-angular/browser .

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
