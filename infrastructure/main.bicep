// infrastructure/main.bicep

/// ==================================================================
/// PARÂMETROS GLOBAIS
/// ==================================================================
@description('O nome base para todos os recursos.')
param projectName string = 'apexfood-dev'

@description('A região do Azure onde os recursos serão implantados.')
param location string = resourceGroup().location

@description('O SKU para o Plano de Serviço (F1 para Gratuito).')
param appServicePlanSku string = 'F1'

@description('O login do administrador SQL. Não é um segredo e pode ter um valor padrão.')
param sqlAdminLogin string = 'apexAdmin'

@description('A senha do administrador SQL. Deve ser passada pelo pipeline de CD e não deve ter valor padrão.')
@secure()
param sqlAdminPassword string

@description('O UPN do administrador do Entra ID (ex: admin@contoso.com).')
param adAdminLogin string

@description('O Object ID do administrador do Entra ID (GUID).')
param adAdminSid string

@description('O Tenant ID do diretório Entra ID. O padrão é o tenant da subscrição atual.')
param adAdminTenantId string = subscription().tenantId

/// ==================================================================
/// VARIÁVEIS DE NOMENCLATURA E AMBIENTE
/// ==================================================================
var appServicePlanName = 'plan-${projectName}'
var appServiceName = 'app-${projectName}'
var appInsightsName = 'appi-${projectName}'
var sqlServerName = 'sql-${projectName}'
var sqlDatabaseName = 'sqldb-${projectName}'
var sqlServerFqdn = '${sqlServerName}${environment().suffixes.sqlServerHostname}'

/// ==================================================================
/// RECURSOS
/// ==================================================================

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: { name: appServicePlanSku }
  kind: 'linux'
  properties: { reserved: true }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: { Application_Type: 'web' }
}

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: { name: 'Basic' }
}

resource sqlServerAdAdmin 'Microsoft.Sql/servers/administrators@2022-08-01-preview' = {
  parent: sqlServer
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: adAdminLogin
    sid: adAdminSid
    tenantId: adAdminTenantId
  }
}

resource sqlServerAdOnlyAuth 'Microsoft.Sql/servers/azureADOnlyAuthentications@2022-08-01-preview' = {
  parent: sqlServer
  name: 'Default'
  properties: {
    azureADOnlyAuthentication: false
  }
}

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
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.properties.ConnectionString }
        { name: 'WEBSITES_PORT', value: '8080' }
        { name: 'ASPNETCORE_URLS', value: 'http://+:8080' }
        { name: 'ConnectionStrings__DefaultConnection', value: 'Server=tcp:${sqlServerFqdn},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;' }
      ]
    }
  }
}

/// ==================================================================
/// SAÍDAS (OUTPUTS)
/// ==================================================================

@description('O nome do host do App Service criado.')
output appServiceHostName string = appService.properties.defaultHostName