FROM node:18-alpine
WORKDIR /app
COPY Samples/TypeScript/Demo/package*.json ./
RUN npm install
COPY Samples/TypeScript/Demo ./
EXPOSE 5000
CMD ["npm","start"]
