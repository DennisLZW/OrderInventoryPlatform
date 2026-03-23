# 环境变量示例

| 文件 | 用途 |
|------|------|
| **`env/.env.docker.example`** | Docker Compose：复制到仓库根目录为 **`.env.docker`**，再运行 `docker compose --env-file .env.docker up --build` |
| **`env/env.azure.example`** | Azure（App Service / Container Apps）：连接串、CORS、迁移等说明（按项配置到门户或 CI/CD 变量） |

ASP.NET Core 在 `docker-compose.yml` 中通过环境变量覆盖配置（如 `ConnectionStrings__DefaultConnection`、`Cors__AllowedOrigins__0`）。  
本地开发也可使用 [User Secrets](https://learn.microsoft.com/dotnet/core/extensions/user-secrets) 或 `appsettings.*.json`（勿提交密钥）。

Azure 部署步骤与 Bicep 模板见 **`infra/azure/README.md`**。
