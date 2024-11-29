# Use the official .NET 8.0 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the .csproj file and restore any dependencies (via `dotnet restore`)
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Build the application
RUN dotnet publish -c Release -o /app/publish

# Use the official .NET 8.0 runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose the port the app runs on (e.g., 80)
EXPOSE 80

# Define the entry point to run the application
ENTRYPOINT ["dotnet", "SS.Gateway.API.dll"]
