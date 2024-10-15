# Step 1: Build the React app
# Use an official Node.js runtime as the base image
FROM node:18-alpine AS build

# Set working directory inside the container
WORKDIR /app

# Copy package.json and package-lock.json to the working directory
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy the rest of the application code to the working directory
COPY . .

# Build the React app for production
RUN npm run build

# Step 2: Serve the app with Nginx
# Use an official nginx image as the base image for the final step
FROM nginx:stable-alpine

# Copy the built React files from the previous stage to Nginx's default HTML directory
COPY --from=build /app/build /usr/share/nginx/html

# Copy a custom Nginx configuration file if needed (optional)
# COPY nginx.conf /etc/nginx/nginx.conf

# Expose port 80 to the outside world
EXPOSE 80

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]
