// Azure Function App for SS.Functions (the SLA breach checker), on the Flex Consumption plan:
// Linux, .NET 8 isolated worker, billed per execution, nothing to pay between timer runs.
//
//   storage account     AzureWebJobsStorage (timer schedule + singleton lease, host keys) and the
//                       "app-package" container the deployment zip is uploaded to
//   log analytics +     logs, traces and the function's invocation history (portal: Function App >
//   application insights  Functions > SlaBreachChecker > Invocations / Monitor)
//   flex consumption plan + function app (with the app settings SS.Functions reads)
//
// Deployed by scripts/deploy-function-app.sh (locally and from Jenkins). Idempotent: re-running updates in place.
//
// The function talks to the same Azure SQL database as the APIs and publishes to Azure Service Bus. A Function
// App outside the AKS cluster can't reach the in-cluster Kafka/RabbitMQ, so the AKS release must also use
// config.messagingProvider: AzureServiceBus for the notification/audit services to receive TicketSlaBreached.

@description('Region. Must offer Flex Consumption: az functionapp list-flexconsumption-locations -o table')
param location string = resourceGroup().location

@description('Function App name - part of <name>.azurewebsites.net, so globally unique.')
@minLength(2)
@maxLength(55) // leaves room for the '-logs' / '-plan' suffixes
param functionAppName string = 'ss-functions-${uniqueString(resourceGroup().id)}'

@description('Storage account name: 3-24 lowercase letters/digits, globally unique.')
@minLength(3)
@maxLength(24)
param storageAccountName string = 'ssfunc${uniqueString(resourceGroup().id)}'

@description('ConnectionStrings:DefaultConnection - the SupportSystem Azure SQL database.')
@secure()
param dbConnectionString string

@description('Azure Service Bus connection string with Manage rights (the publisher creates its topic).')
@secure()
param serviceBusConnectionString string

@description('NCRONTAB schedule of the SLA breach checker. "0 */5 * * * *" = every 5 minutes.')
param slaBreachCheckerSchedule string = '0 */5 * * * *'

@description('Most tickets reported per run.')
param slaBreachCheckerBatchSize int = 100

@description('Memory per instance (MB): 512, 2048 or 4096. A timer job needs little.')
@allowed([512, 2048, 4096])
param instanceMemoryMB int = 512

param tags object = {
  app: 'supportsystem'
  component: 'ss-functions'
}

var deploymentContainerName = 'app-package'

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: { name: 'Standard_LRS' }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }

  resource blobService 'blobServices' = {
    name: 'default'

    resource deploymentContainer 'containers' = {
      name: deploymentContainerName
    }
  }
}

// Key-based for simplicity (no role assignments needed, so the deploying identity only needs Contributor).
// Production hardening: managed identity - AzureWebJobsStorage__accountName + Storage Blob Data Owner role.
var storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storage.name};AccountKey=${storage.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${functionAppName}-logs'
  location: location
  tags: tags
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${functionAppName}-ai'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: '${functionAppName}-plan'
  location: location
  tags: tags
  kind: 'functionapp'
  sku: {
    tier: 'FlexConsumption'
    name: 'FC1'
  }
  properties: {
    reserved: true // Linux
  }
}

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  tags: tags
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      minTlsVersion: '1.2'
      // Flex Consumption takes the runtime from functionAppConfig below, so no FUNCTIONS_WORKER_RUNTIME /
      // FUNCTIONS_EXTENSION_VERSION settings (they're rejected on this plan).
      appSettings: [
        { name: 'AzureWebJobsStorage', value: storageConnectionString }
        { name: 'DEPLOYMENT_STORAGE_CONNECTION_STRING', value: storageConnectionString }
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.properties.ConnectionString }
        { name: 'SlaBreachCheckerSchedule', value: slaBreachCheckerSchedule }
        { name: 'SlaBreachChecker__BatchSize', value: string(slaBreachCheckerBatchSize) }
        { name: 'ConnectionStrings__DefaultConnection', value: dbConnectionString }
        { name: 'Messaging__Provider', value: 'AzureServiceBus' }
        { name: 'Messaging__AzureServiceBus__ConnectionString', value: serviceBusConnectionString }
        { name: 'Messaging__AzureServiceBus__AutoCreateEntities', value: 'true' }
      ]
    }
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storage.properties.primaryEndpoints.blob}${deploymentContainerName}'
          authentication: {
            type: 'StorageAccountConnectionString'
            storageAccountConnectionStringName: 'DEPLOYMENT_STORAGE_CONNECTION_STRING'
          }
        }
      }
      scaleAndConcurrency: {
        maximumInstanceCount: 40 // the plan's minimum; the timer runs on one instance anyway
        instanceMemoryMB: instanceMemoryMB
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '8.0'
      }
    }
  }
}

output functionAppName string = functionApp.name
output functionAppHostName string = functionApp.properties.defaultHostName
output appInsightsName string = appInsights.name
