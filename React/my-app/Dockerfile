FROM node:26.8.1-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
CMD [ "npm", "run", "dev", "--", "--host", "0.0.0.0" ]