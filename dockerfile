# Multi-stage build Dockerfile for Assignment_Example_HU
#1. Restore & build
FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0 AS build
WORKDIR /src

# Copy csproj separately for efficient layer caching
COPY Assignment_Example_HU.csproj ./
RUN dotnet restore Assignment_Example_HU.csproj

# Copy the remaining source
COPY . .

# Publish (no self-contained app host for smaller image)
RUN dotnet publish Assignment_Example_HU.csproj -c Release -o /app/publish /p:UseAppHost=false


#2. Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Optionally configure ASP.NET Core URLs (container listens on 8080 internally)
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Copy published output
COPY --from=build /app/publish .

# Entry point

ENTRYPOINT ["dotnet", "Assignment_Example_HU.dll"]