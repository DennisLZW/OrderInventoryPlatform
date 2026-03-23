// Order Inventory Platform — Azure Database for PostgreSQL Flexible Server + Container Apps
//
// Prerequisites: resource group, Azure CLI, Bicep (`az bicep install`)
//
//   az group create -n rg-oip-prod -l eastus
//   az deployment group create -g rg-oip-prod -f main.bicep \
//     -p postgresAdminPassword='<secure-password>' \
//     -p apiImage='<registry>/order-inventory-api:latest'
//
// Push `backend/Dockerfile` to ACR (or another registry) before deploy; image must expose port 8080.

targetScope = 'resourceGroup'

@description('Prefix for resource names (lowercase letters/numbers).')
@minLength(3)
@maxLength(12)
param namePrefix string = 'oip'

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('PostgreSQL administrator login (Flexible Server naming rules apply).')
param postgresAdminLogin string = 'oipadmin'

@secure()
@description('PostgreSQL administrator password.')
param postgresAdminPassword string

@description('Application database name.')
param databaseName string = 'order_inventory_db'

@description('Container image for the API (e.g. myregistry.azurecr.io/order-inventory-api:latest).')
param apiImage string

// --- PostgreSQL Flexible Server (Npgsql: use Ssl Mode=Require) ---
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: '${namePrefix}-pg-${uniqueString(resourceGroup().id)}'
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: postgresAdminLogin
    administratorLoginPassword: postgresAdminPassword
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
  }
}

resource postgresFirewallAzure 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = {
  parent: postgresServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgresServer
  name: databaseName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
  dependsOn: [
    postgresFirewallAzure
  ]
}

var connectionString = 'Host=${postgresServer.properties.fullyQualifiedDomainName};Port=5432;Database=${databaseName};Username=${postgresAdminLogin};Password=${postgresAdminPassword};Ssl Mode=Require;Trust Server Certificate=false'

// --- Container Apps ---
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${namePrefix}-logs'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource containerAppEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: '${namePrefix}-cae'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
  dependsOn: [
    logAnalytics
    postgresDatabase
  ]
}

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: '${namePrefix}-api'
  location: location
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
      }
      secrets: [
        {
          name: 'connectionstring'
          value: connectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiImage
          env: [
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'connectionstring'
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
              name: 'DOTNET_RUNNING_IN_CONTAINER'
              value: 'true'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 5
      }
    }
  }
  dependsOn: [
    containerAppEnv
    postgresDatabase
  ]
}

@description('PostgreSQL Flexible Server host (FQDN).')
output postgresFqdn string = postgresServer.properties.fullyQualifiedDomainName

output postgresDatabaseName string = databaseName

@description('Public HTTPS URL for the Container App.')
output apiUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'

@description('Template for manual App Service / pipeline configuration (replace placeholders).')
output connectionStringTemplate string = 'Host=<FQDN>;Port=5432;Database=${databaseName};Username=${postgresAdminLogin};Password=<PASSWORD>;Ssl Mode=Require;Trust Server Certificate=false'
