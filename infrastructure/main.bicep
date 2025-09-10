// infrastructure/main.bicep

/// <summary>
/// Parâmetro para o nome base do projeto, usado para nomear todos os recursos.
/// </summary>
@description('The base name for all resources.')
param projectName string

/// <summary>
/// Parâmetro para a localização dos recursos no Azure.
/// </summary>
@description('The Azure region where the resources will be deployed.')
param location string = resourceGroup().location

/// <summary>
/// Parâmetro para o SKU do Plano de Serviço (ex: F1 para Free, B1 para Basic).
/// </summary>
@description('The SKU for the App Service Plan.')
param appServicePlanSku string = 'F1'

// Variáveis para construir os nomes dos recursos de forma padronizada.
// CORRIGIDO: Nomes baseados diretamente no projectName para consistência.
var appServicePlanName = 'plan-${projectName}'
var appInsightsName = 'appi-${projectName}'

// Recurso: Plano de Serviço do Azure
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

// Recurso: Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

// Recurso: App Service
resource appService 'Microsoft.Web/sites@2022-09-01' = {
  // CORRIGIDO: Usa o projectName diretamente, sem adicionar prefixos.
  name: projectName
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
      ]
    }
  }
}

/// <summary>
/// Saída (Output): O nome do host do App Service criado.
/// </summary>
@description('The hostname of the deployed App Service.')
output appServiceHostName string = appService.properties.defaultHostName