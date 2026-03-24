# Environment variable examples

| File                          | Purpose                                                                                                                                 |
| ----------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| **`env/.env.docker.example`** | Docker Compose: copy to the repo root as **`.env.docker`**, then run `docker compose --env-file .env.docker up --build`                 |
| **`env/env.azure.example`**   | Azure (App Service / Container Apps): connection strings, CORS, migrations, etc. (configure each item in the portal or CI/CD variables) |

ASP.NET Core reads overrides from environment variables in `docker-compose.yml` (e.g. `ConnectionStrings__DefaultConnection`, `Cors__AllowedOrigins__0`).  
For local development you can also use [User Secrets](https://learn.microsoft.com/dotnet/core/extensions/user-secrets) or `appsettings.*.json` (do not commit secrets).

Azure deployment steps and Bicep templates are in **`infra/azure/README.md`**.

## Deployed app screenshots

### Dashboard
![Dashboard](/Users/dennis/.cursor/projects/Users-dennis-OrderInventoryPlatform/assets/9a21d4e9-bd0c-4fbb-829a-3ddda3385a5d-8bf63547-bffb-4681-920e-6703f8d793e7.png)

### Products
![Products](/Users/dennis/.cursor/projects/Users-dennis-OrderInventoryPlatform/assets/556b50d9-16fb-4f36-9f21-85563993f0e2-81c030fd-5cae-45ac-8f8d-68c34c535ca0.png)

### Inventory
![Inventory](/Users/dennis/.cursor/projects/Users-dennis-OrderInventoryPlatform/assets/e25558e0-656d-49de-a7c4-92ce051eb35c-1b7b8ba8-b6ab-41d7-a6b6-baa584e27473.png)

### Create Order
![Create Order](/Users/dennis/.cursor/projects/Users-dennis-OrderInventoryPlatform/assets/460b6790-7c19-45ef-aabe-47da932be7ba-7640f511-12df-4260-9e21-02bb53106c51.png)

### Orders
![Orders](/Users/dennis/.cursor/projects/Users-dennis-OrderInventoryPlatform/assets/fe0bb2b2-19ce-41b7-9123-249825503eba-d3fb670c-2ed8-465c-811d-314adcd0ca15.png)
