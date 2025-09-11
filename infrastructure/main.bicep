// infrastructure/main.bicep

@description('O nome base para todos os recursos.')
param projectName string = 'apexfood'

@description('A região do Azure onde os recursos serão implantados.')
param location string = resourceGroup().location

@description('O SKU do Plano de Serviço (ex: F1 para Free, B1 para Basic).')
param appServicePlanSku string = 'F1'

// NOVO PARÂMETRO: A senha para o administrador do SQL Server. Será passada pelo pipeline de forma segura.
@description('A senha do administrador do SQL Server.')
@secure()
param sqlAdminPassword string

// --- Variáveis ---
var appServicePlanName = 'plan-${projectName}'
var appServiceName = 'app-${projectName}'
var appInsightsName = 'appi-${projectName}'
var sqlServerName = 'sql-${projectName}-${uniqueString(resourceGroup().id)}' // Garante um nome de servidor SQL globalmente único.
var sqlDatabaseName = 'sqldb-${projectName}'
var sqlAdminLogin = 'apexadmin'

// --- Recurso: Plano de Serviço ---
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSku
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// --- Recurso: Application Insights ---
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

// ==================================================================
// NOVOS RECURSOS: Servidor SQL e Banco de Dados
// ==================================================================
resource sqlServer 'Microsoft.Sql/servers@2022-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'User'
      login: 'welly.barbosa1@gmail.com' // Seu e-mail de login do Azure AD
      sid: '5845f6bc-2a4f-47b3-b13e-4a06a521339c'         // O Object ID que você copiou
      tenantId: 'f5cd57c5-906e-41ca-a224-e573e36a8b2f'     // O Tenant ID que você copiou
      azureADOnlyAuthentication: false // Permite tanto login SQL quanto AD
    }
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-08-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Basic' // Usando um SKU básico e de baixo custo para desenvolvimento.
    tier: 'Basic'
    capacity: 5 // DTUs
  }
}
// ==================================================================

// --- Recurso: App Service (Atualizado) ---
resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: appServiceName
  location: location
  kind: 'app'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'WEBSITES_PORT'
          value: '8080'
        }
        {
          name: 'ASPNETCORE_URLS'
          value: 'http://+:8080'
        }
        // ==================================================================
        // ATUALIZAÇÃO: Adiciona a Connection String do banco de dados dinamicamente.
        // ==================================================================
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabase.name};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
      ]
    }
  }
}

@description('O nome do host do App Service criado.')
output appServiceHostName string = appService.properties.defaultHostName