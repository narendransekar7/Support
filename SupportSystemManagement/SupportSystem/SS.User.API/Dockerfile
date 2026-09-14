# Use the official .NET 8.0 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Build context is the solution root (SS.User.API depends on the SS.Base.* projects as siblings)
WORKDIR /src

# Copy csproj files first and restore (better layer caching)
COPY SS.User.API/SS.User.API.csproj SS.User.API/
COPY SS.Base.Application/SS.Base.Application.csproj SS.Base.Application/
COPY SS.Base.Infrastructure/SS.Base.Infrastructure.csproj SS.Base.Infrastructure/
COPY SS.Base.Domain/SS.Base.Domain.csproj SS.Base.Domain/
RUN dotnet restore SS.User.API/SS.User.API.csproj

# Copy the rest of the source
COPY SS.User.API/ SS.User.API/
COPY SS.Base.Application/ SS.Base.Application/
COPY SS.Base.Infrastructure/ SS.Base.Infrastructure/
COPY SS.Base.Domain/ SS.Base.Domain/

# Build the application
RUN dotnet publish SS.User.API/SS.User.API.csproj -c Release -o /app/publish

# Use the official .NET 8.0 runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Define the entry point to run the application
ENTRYPOINT ["dotnet", "SS.User.API.dll"]
