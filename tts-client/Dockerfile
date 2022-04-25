FROM node:16.10.0-slim as build
WORKDIR /tts-client
COPY src ./src
COPY *.json ./
RUN npm install -g @angular/cli@13.3.1
RUN npm install
RUN ng build --prod

FROM nginx:alpine as deploy
WORKDIR /tts-client
COPY --from=build /tts-client/dist/tts-client/*.* /usr/share/nginx/html/
