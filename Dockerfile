# Dockerfile for Event Registration System
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["WebApplication1/WebApplication1.csproj", "WebApplication1/"]
COPY ["EventRegistration.Application/EventRegistration.Application.csproj", "EventRegistration.Application/"]
COPY ["EventRegistration.Domain/EventRegistration.Domain.csproj", "EventRegistration.Domain/"]
COPY ["EventRegistration.Infrastructure/EventRegistration.Infrastructure.csproj", "EventRegistration.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "WebApplication1/WebApplication1.csproj"

# Copy source code
COPY . .

# Build application
WORKDIR "/src/WebApplication1"
RUN dotnet build "WebApplication1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApplication1.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/data

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "WebApplication1.dll"]
