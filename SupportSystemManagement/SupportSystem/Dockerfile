#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base image to run the WebAPI (.NET Core 3.1)
FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage for .NET Core 3.1 (WebAPI project)
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY . ./

# Restore and build the WebAPI (.NET Core 3.1)
WORKDIR /src/SupportSystemManagement/SupportSystem/SupportSystem.WebAPI/SupportSystem.WebAPI
RUN dotnet restore "SupportSystem.WebAPI.csproj"
RUN dotnet build "SupportSystem.WebAPI.csproj" -c Release -o /app/build

# Publish the WebAPI project
FROM build AS publish
RUN dotnet publish "SupportSystem.WebAPI.csproj" -c Release -o /app/publish



# Final image to run the WebAPI project
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SupportSystem.WebAPI.dll"]
