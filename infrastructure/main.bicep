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

/// ==================================================================
/// PARÂMETROS E SEGREDOS PARA O BANCO DE DADOS
/// ==================================================================

@description('O login do administrador SQL. Deve ser passado pelo pipeline de CD.')
@secure()
param sqlAdminLogin string = 'apexAdmin' // Valor padrão para validação local

@description('A senha do administrador SQL. Deve ser passada pelo pipeline de CD.')
@secure()
param sqlAdminPassword string

@description('O UPN do administrador do Entra ID.')
param adAdminLogin string

@description('O Object ID do administrador do Entra ID.')
param adAdminSid string

@description('O Tenant ID do diretório Entra ID.')
param adAdminTenantId string

/// ==================================================================
/// VARIÁVEIS DE NOMENCLATURA
/// ==================================================================

var appServicePlanName = 'plan-${projectName}'
var appServiceName = 'app-${projectName}'
var appInsightsName = 'appi-${projectName}'
var sqlServerName = 'sql-${projectName}'
var sqlDatabaseName = 'sqldb-${projectName}'

/// ==================================================================
/// RECURSOS DE COMPUTAÇÃO E MONITORAMENTO
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
        { name: 'ConnectionStrings__DefaultConnection', value: 'Server=tcp:${sqlServer.name}.database.windows.net,1433;Initial Catalog=${sqlDatabase.name};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;' }
      ]
    }
  }
}

/// ==================================================================
/// RECURSOS DE BANCO DE DADOS (ESTRUTURA CORRIGIDA)
/// ==================================================================

// 1. Cria o servidor SQL com o administrador SQL
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
  }
}

// 2. Cria o banco de dados dentro do servidor
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: { name: 'Basic' }
}

// 3. Configura o administrador do Entra ID como um recurso filho
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

// 4. Configura a política de autenticação como um recurso filho
resource sqlServerAdOnlyAuth 'Microsoft.Sql/servers/azureADOnlyAuthentications@2022-08-01-preview' = {
  parent: sqlServer
  name: 'Default'
  properties: {
    azureADOnlyAuthentication: false
  }
}

/// ==================================================================
/// SAÍDAS (OUTPUTS)
/// ==================================================================

@description('O nome do host do App Service criado.')
output appServiceHostName string = appService.properties.defaultHostName