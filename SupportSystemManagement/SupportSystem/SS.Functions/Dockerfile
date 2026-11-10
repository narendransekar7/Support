# Build context is the solution root (SS.Functions depends on the SS.Base.* projects as siblings).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SS.Functions/SS.Functions.csproj SS.Functions/
COPY SS.Base.Infrastructure/SS.Base.Infrastructure.csproj SS.Base.Infrastructure/
COPY SS.Base.Domain/SS.Base.Domain.csproj SS.Base.Domain/
COPY SS.Base.Messaging/SS.Base.Messaging.csproj SS.Base.Messaging/
RUN dotnet restore SS.Functions/SS.Functions.csproj

COPY SS.Functions/ SS.Functions/
COPY SS.Base.Infrastructure/ SS.Base.Infrastructure/
COPY SS.Base.Domain/ SS.Base.Domain/
COPY SS.Base.Messaging/ SS.Base.Messaging/
RUN dotnet publish SS.Functions/SS.Functions.csproj -c Release -o /home/site/wwwroot

# The Azure Functions runtime image: it contains the Functions host, which loads the app from
# /home/site/wwwroot and starts the isolated worker (SS.Functions.dll). Same image Azure uses for
# container-based function apps (Container Apps, Premium/Flex plans).
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true
COPY --from=build /home/site/wwwroot /home/site/wwwroot
