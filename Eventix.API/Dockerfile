# Build context must be the solution folder (the one with Eventix.slnx), because the API references the other projects:
#   docker build -f Eventix.API/Dockerfile -t eventix-api .

# ---- Stage 1: build and publish with the full SDK ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project files first, so "dotnet restore" is cached until a package changes.
COPY Eventix.Domain/Eventix.Domain.csproj Eventix.Domain/
COPY Eventix.Application/Eventix.Application.csproj Eventix.Application/
COPY Eventix.Infrastructure/Eventix.Infrastructure.csproj Eventix.Infrastructure/
COPY Eventix.API/Eventix.API.csproj Eventix.API/
RUN dotnet restore Eventix.API/Eventix.API.csproj

# The test project is not needed to run the server, so it is not copied.
COPY Eventix.Domain/ Eventix.Domain/
COPY Eventix.Application/ Eventix.Application/
COPY Eventix.Infrastructure/ Eventix.Infrastructure/
COPY Eventix.API/ Eventix.API/
RUN dotnet publish Eventix.API/Eventix.API.csproj -c Release -o /app/publish --no-restore

# ---- Stage 2: small runtime image ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# tzdata gives the container the IANA time zones (the business clock uses "Asia/Jerusalem").
# It is already part of this Debian image; the install is a no-op safety net.
RUN apt-get update \
    && apt-get install -y --no-install-recommends tzdata \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# NLog writes a daily file under logs/; the non-root "app" user must be allowed to create it.
RUN mkdir -p /app/logs && chown -R app:app /app/logs
USER app

# Render passes the port to listen on in $PORT; locally it falls back to 8080.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} exec dotnet Eventix.API.dll"]
