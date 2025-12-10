//the master orchestrator

// 1. Set scope
targetScope = 'resourceGroup'

// 2. Parameters (inputs)
@description('The location where resources will be deployed.')
param location string = resourceGroup().location

@description('The environment name (e.g., dev, prod).')
param environmentName string = 'dev'

@description('The name of the application.')
param applicationName string = 'modern-stoic'

//adminPassword parameter (not needed for NoSQL)
// @secure()
// @description('The administrator password for the Cosmos DB.')
// param adminPassword string

//3. Variables

// Fix: Shorten the unique string to 5 characters to fit within the 32-char limit
var uniqueSuffix = substring(uniqueString(resourceGroup().id), 0, 5)
var resourceToken = toLower('${applicationName}-${environmentName}-${uniqueSuffix}')

var acrName = replace('acr-${resourceToken}', '-', '') // ACR names must be alphanumeric only
var cosmosName = 'cosmos-${resourceToken}'
var acaEnvName = 'aca-env-${resourceToken}'
var appName = 'app-${resourceToken}'

// Module 1: Azure Container Registry
module acr 'modules/security/acr.bicep' = {
  name: 'acrDeployment'
  params: {
    location: location
    acrName: acrName
  }
}

// Module 2: Cosmos DB (Mongo vCore)
module cosmos 'modules/data/cosmos-nosql.bicep' = {
  name: 'cosmosDeployment'
  params: {
    location: location
    accountName: cosmosName
    //adminPassword: adminPassword
  }
}

// Module 3: Azure Container Apps Environment & App

module containerApp 'modules/compute/container-app.bicep' = {
  name: 'containerAppDeployment'
  params: {
    location: location
    environmentName: acaEnvName
    appName: appName
    // DEPENDENCY: We pass the ACR login server from Module 1 to here
    acrLoginServer: acr.outputs.loginServer
    // DEPENDENCY: We pass the ACR password (secret) from Module 1 to here
    acrName: acrName
    // We must pass the password output from the ACR module
    acrPassword: acr.outputs.adminPassword
    // DEPENDENCY: We pass the DB Connection string from Module 2 to here
    cosmosConnectionString: cosmos.outputs.connectionString
  }
}
