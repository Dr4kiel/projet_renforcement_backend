# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY AgentControlService/*.csproj ./AgentControlService/
RUN dotnet restore AgentControlService/AgentControlService.csproj

# Copy everything else and build
COPY AgentControlService/. ./AgentControlService/
RUN dotnet publish AgentControlService/AgentControlService.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5003

ENV ASPNETCORE_URLS=http://+:5003
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AgentControlService.dll"]
