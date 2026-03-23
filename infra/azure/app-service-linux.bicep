// Linux Web App running a container image (Azure App Service).
// Use after you have a PostgreSQL Flexible Server and pushed your API image to ACR (or Docker Hub with credentials).
//
// Example:
//   az deployment group create -g rg-oip-prod -f app-service-linux.bicep \
//     -p appName='oip-api-prod' \
//     -p containerImage='myregistry.azurecr.io/order-inventory-api:latest' \
//     -p connectionString='<Npgsql connection string with Ssl Mode=Require>'

targetScope = 'resourceGroup'

@description('App Service name (globally unique, e.g. oip-api-prod-xyz).')
param appName string

@description('Region.')
param location string = resourceGroup().location

@description('Linux App Service Plan SKU (B1 = Basic dev/test).')
param planSku string = 'B1'

@description('Full container image reference (ACR or public registry).')
param containerImage string

@secure()
@description('Full Npgsql connection string for Azure PostgreSQL (include Ssl Mode=Require).')
param connectionString string

resource plan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: '${appName}-plan'
  location: location
  sku: {
    name: planSku
    tier: 'Basic'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: appName
  location: location
  kind: 'app,linux,container'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|${containerImage}'
      alwaysOn: planSku != 'F1'
      http20Enabled: true
      appSettings: [
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'WEBSITES_PORT'
          value: '8080'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: ''
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: connectionString
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'Database__ApplyMigrationsOnStartup'
          value: 'true'
        }
        {
          name: 'WEBSITE_HTTPLOGGING_RETENTION_DAYS'
          value: '3'
        }
      ]
    }
  }
}

@description('Default hostname for the Web App.')
output defaultHostname string = webApp.properties.defaultHostName
