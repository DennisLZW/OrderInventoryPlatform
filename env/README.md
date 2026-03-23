# Environment variable examples

| File | Purpose |
|------|---------|
| **`env/.env.docker.example`** | Docker Compose: copy to the repo root as **`.env.docker`**, then run `docker compose --env-file .env.docker up --build` |
| **`env/env.azure.example`** | Azure (App Service / Container Apps): connection strings, CORS, migrations, etc. (configure each item in the portal or CI/CD variables) |

ASP.NET Core reads overrides from environment variables in `docker-compose.yml` (e.g. `ConnectionStrings__DefaultConnection`, `Cors__AllowedOrigins__0`).  
For local development you can also use [User Secrets](https://learn.microsoft.com/dotnet/core/extensions/user-secrets) or `appsettings.*.json` (do not commit secrets).

Azure deployment steps and Bicep templates are in **`infra/azure/README.md`**.
