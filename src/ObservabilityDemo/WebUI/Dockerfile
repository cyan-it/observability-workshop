FROM node:22-alpine@sha256:ad1aedbcc1b0575074a91ac146d6956476c1f9985994810e4ee02efd932a68fd AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .

RUN npm run build

FROM nginx:alpine@sha256:65645c7bb6a0661892a8b03b89d0743208a18dd2f3f17a54ef4b76fb8e2f2a10
COPY --from=build /app/dist/webui/browser /usr/share/nginx/html
RUN rm /etc/nginx/conf.d/default.conf
COPY etc/nginx/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
