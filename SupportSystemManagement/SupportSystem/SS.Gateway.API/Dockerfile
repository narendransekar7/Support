# Use the official .NET 8.0 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Build context is the solution root (SS.Gateway.API depends on SS.Base.Observability and SS.Base.Authentication as siblings)
WORKDIR /src

# Copy csproj files first and restore (better layer caching)
COPY SS.Gateway.API/SS.Gateway.API.csproj SS.Gateway.API/
COPY SS.Base.Authentication/SS.Base.Authentication.csproj SS.Base.Authentication/
COPY SS.Base.Observability/SS.Base.Observability.csproj SS.Base.Observability/
RUN dotnet restore SS.Gateway.API/SS.Gateway.API.csproj

# Copy the rest of the source
COPY SS.Gateway.API/ SS.Gateway.API/
COPY SS.Base.Authentication/ SS.Base.Authentication/
COPY SS.Base.Observability/ SS.Base.Observability/

# Build the application
RUN dotnet publish SS.Gateway.API/SS.Gateway.API.csproj -c Release -o /app/publish

# Use the official .NET 8.0 runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Define the entry point to run the application
ENTRYPOINT ["dotnet", "SS.Gateway.API.dll"]
