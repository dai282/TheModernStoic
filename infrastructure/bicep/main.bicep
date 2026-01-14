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

@description('The tag/version of the docker image to deploy')
param imageTag string = 'latest'

@description('The Docker Hub username for the image repository')
param dockerHubUsername string = 'dai282'

@secure()
@description('The API Key for Hugging Face Inference')
param huggingFaceApiKey string // <--- NEW PARAMETER

@description('The Auth0 Domain')
param auth0Domain string // <--- NEW PARAMETER

@description('The Auth0 Audience')
param auth0Audience string // <--- NEW PARAMETER

//adminPassword parameter (not needed for NoSQL)
// @secure()
// @description('The administrator password for the Cosmos DB.')
// param adminPassword string

//3. Variables

// Fix: Shorten the unique string to 5 characters to fit within the 32-char limit
var uniqueSuffix = substring(uniqueString(resourceGroup().id), 0, 5)
var resourceToken = toLower('${applicationName}-${environmentName}-${uniqueSuffix}')

var cosmosName = 'cosmos-${resourceToken}'
var acaEnvName = 'aca-env-${resourceToken}'
var appName = 'app-${resourceToken}'

// Module 1: Cosmos DB (NoSQL)
module cosmos 'modules/data/cosmos-nosql.bicep' = {
  name: 'cosmosDeployment'
  params: {
    location: location
    accountName: cosmosName
    //adminPassword: adminPassword
  }
}

// Module 2: Azure Container Apps Environment & App
module containerApp 'modules/compute/container-app.bicep' = {
  name: 'containerAppDeployment'
  params: {
    location: location
    environmentName: acaEnvName
    appName: appName
    dockerHubUsername: dockerHubUsername
    // DEPENDENCY: We pass the DB Connection string from Module 1 to here
    cosmosConnectionString: cosmos.outputs.connectionString
    imageTag: imageTag
    huggingFaceApiKey: huggingFaceApiKey // <--- PASS IT DOWN
    auth0Domain: auth0Domain // <--- PASS IT DOWN
    auth0Audience: auth0Audience // <--- PASS IT DOWN
  }
}
