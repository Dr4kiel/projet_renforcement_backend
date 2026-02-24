# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY RealtimeService/*.csproj ./RealtimeService/
RUN dotnet restore RealtimeService/RealtimeService.csproj

# Copy everything else and build
COPY RealtimeService/. ./RealtimeService/
RUN dotnet publish RealtimeService/RealtimeService.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy the published app from build stage
COPY --from=build /app/publish .

# Expose port
EXPOSE 5002

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5002
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "RealtimeService.dll"]
