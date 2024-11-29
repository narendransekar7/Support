# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory in the container
WORKDIR /src

# Copy the .csproj file and restore any dependencies (via `dotnet restore`)
COPY SS.Web.API/SS.Web.API.csproj SS.Web.API/
RUN dotnet restore SS.Web.API/SS.Web.API.csproj

# Copy the rest of the application code
COPY . .

# Publish the application to the /app/publish directory in release mode
RUN dotnet publish SS.Web.API/SS.Web.API.csproj -c Release -o /app/publish

# Use the official .NET ASP.NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Set the working directory in the container
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose the port the app will run on (port 80)
EXPOSE 80

# Define the entry point to run the application
ENTRYPOINT ["dotnet", "SS.Web.API.dll"]
