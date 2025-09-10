// infrastructure/main.bicep

/// <summary>
/// Parâmetro para o nome base do projeto, usado para nomear todos os recursos.
/// </summary>
@description('The base name for all resources.')
param projectName string = 'apexfood'

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
var appServicePlanName = 'plan-${projectName}'
var appServiceName = 'app-${projectName}'
var appInsightsName = 'appi-${projectName}'

// Recurso: Plano de Serviço do Azure
// Define a capacidade computacional (CPU/memória) para nossa aplicação.
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSku
  }
  kind: 'linux' // Vamos usar Linux, que é mais econômico e o padrão para .NET.
  properties: {
    reserved: true // Necessário para Linux SKUs.
  }
}

// Recurso: Application Insights
// Coleta telemetria, logs e monitora a performance da nossa API.
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

// Recurso: App Service
// É o serviço que efetivamente hospedará e executará nossa API .NET.
resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: appServiceName
  location: location
  kind: 'app'
  properties: {
    serverFarmId: appServicePlan.id // Associa o App Service ao Plano de Serviço.
    https:Only: true // Força o uso de HTTPS.
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0' // Especifica o runtime da nossa aplicação.
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString // Conecta a API ao App Insights.
        }
      ]
    }
  }
}

/// <summary>
/// Saída (Output): O nome do host do App Service criado.
/// O pipeline de CD usará essa informação para saber onde implantar a aplicação.
/// </summary>
@description('The hostname of the deployed App Service.')
output appServiceHostName string = appService.properties.defaultHostName