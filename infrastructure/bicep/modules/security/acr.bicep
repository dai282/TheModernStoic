//Defines Azure container registry

//INPUTS
param location string
param acrName string

//RESOURCE DEFINITION
resource acr 'Microsoft.ContainerRegistry/registries@2025-11-01' = {
  name: acrName
  location: location
  sku: { 
    name: 'Basic' 
  }
  properties: {
    adminUserEnabled: true //Enables username/password auth
  }
}

//OUTPUTS
output loginServer string = acr.properties.loginServer

// SECURITY NOTE: This retrieves the actual password.
// We use 'secure string' so it doesn't show up in plain text in logs.
@description('The admin password for the container registry.')
@secure()
output adminPassword string = acr.listCredentials().passwords[0].value //this is passed to main.bicep
