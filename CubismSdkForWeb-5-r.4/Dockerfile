FROM node:18-alpine

# Set the working directory to the sample application
WORKDIR /app/Samples/TypeScript/Demo

# Install the demo's dependencies
COPY Samples/TypeScript/Demo/package*.json ./
RUN npm install

# Copy the Cubism SDK resources used by the demo
COPY Core /app/Core
COPY Framework /app/Framework
COPY Samples/Resources /app/Samples/Resources
COPY Samples/TypeScript/Demo /app/Samples/TypeScript/Demo

EXPOSE 5000

# Start the application
CMD ["npm", "start"]
