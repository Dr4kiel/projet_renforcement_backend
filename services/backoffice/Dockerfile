# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY BackOfficeService/*.csproj ./BackOfficeService/
RUN dotnet restore BackOfficeService/BackOfficeService.csproj

# Copy everything else and build
COPY BackOfficeService/. ./BackOfficeService/
RUN dotnet publish BackOfficeService/BackOfficeService.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Copy the published app from build stage
COPY --from=build /app/publish .

# Expose port
EXPOSE 5001

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "BackOfficeService.dll"]
