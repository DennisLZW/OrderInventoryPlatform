# Azure deployment (App Service / Container Apps + PostgreSQL Flexible Server)

This stack is **PostgreSQL-compatible with [Azure Database for PostgreSQL – Flexible Server](https://learn.microsoft.com/azure/postgresql/flexible-server/)**.  
Use **Npgsql** connection strings with **`Ssl Mode=Require`** (and typically `Trust Server Certificate=false`) — the templates below include this.

## What’s in this folder

| File | Purpose |
|------|---------|
| **`main.bicep`** | PostgreSQL Flexible Server + Log Analytics + Container Apps Environment + Container App (API) |
| **`app-service-linux.bicep`** | Linux **App Service** running the same **container** image + app settings |
| **`main.parameters.json.example`** | Example parameters for `main.bicep` |

## Prerequisites

- Azure subscription
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (`az login`)
- Bicep: `az bicep install`
- A container image built from `backend/Dockerfile` and pushed to **Azure Container Registry** (or another registry the platform can pull from)

### Build & push the API image (example)

```bash
az acr login --name <your-acr>
docker build -t <your-acr>.azurecr.io/order-inventory-api:latest ./backend
docker push <your-acr>.azurecr.io/order-inventory-api:latest
```

## Option A — Deploy with `main.bicep` (Container Apps + PostgreSQL)

Creates:

- **PostgreSQL Flexible Server** (Burstable `Standard_B1ms`, PostgreSQL 16)
- Firewall rule **AllowAzureServices** (required for PaaS connectivity)
- **Log Analytics** + **Container Apps** environment + **Container App** with env vars and DB secret

```bash
RESOURCE_GROUP=rg-oip-prod
LOCATION=eastus

az group create --name "$RESOURCE_GROUP" --location "$LOCATION"

az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file infra/azure/main.bicep \
  --parameters \
    postgresAdminPassword='<strong-password>' \
    apiImage='<your-acr>.azurecr.io/order-inventory-api:latest'
```

Outputs include **`apiUrl`** (HTTPS) and **`postgresFqdn`**.

### Health probes

The API exposes:

- `GET /health` — overall health
- `GET /health/ready` — DB readiness (EF Core context)

Point Container App ingress health probes at `/health` or `/health/ready` if you add probes in the portal.

## Option B — Linux App Service (container)

Use **`app-service-linux.bicep`** when you already have a PostgreSQL server and image, or after `main.bicep` if you prefer App Service instead of Container Apps (different topology).

```bash
az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file infra/azure/app-service-linux.bicep \
  --parameters \
    appName='oip-api-<unique>' \
    containerImage='<your-acr>.azurecr.io/order-inventory-api:latest' \
    connectionString='Host=<server>.postgres.database.azure.com;Port=5432;Database=order_inventory_db;Username=oipadmin;Password=<pwd>;Ssl Mode=Require;Trust Server Certificate=false'
```

**Private ACR**: configure **Container settings** / registry credentials in the portal or add `DOCKER_REGISTRY_SERVER_*` app settings (see Microsoft docs).

**Port**: `WEBSITES_PORT=8080` is set in the template to match `backend/Dockerfile`.

## Application configuration (environment variables)

See **`env/env.azure.example`** in the repo root for common settings:

- `ConnectionStrings__DefaultConnection` — must include **`Ssl Mode=Require`** for Azure PostgreSQL
- `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, … — your frontend URLs
- `Database__ApplyMigrationsOnStartup` — optional; prefer running migrations in CI/CD for production

## Runtime behavior (code)

- **`AzureHostingExtensions`**: enables **forwarded headers** on **Azure App Service** (`WEBSITE_INSTANCE_ID`) and **Azure Container Apps** (`CONTAINER_APP_NAME`), or when `Azure:UseForwardedHeaders` is `true`.
- **Health checks**: EF Core `ApplicationDbContext` is registered for `/health` and `/health/ready`.

## Cost & SKUs

Templates use **small SKUs** for getting started. Adjust `Standard_B1ms`, Container Apps scale, and App Service `planSku` for production workloads and SLAs.

## Validate Bicep locally

```bash
az bicep build --file infra/azure/main.bicep
az bicep build --file infra/azure/app-service-linux.bicep
```
