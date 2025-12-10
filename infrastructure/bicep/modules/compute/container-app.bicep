//Defines Azure Container App environement and the app itself
param location string
param environmentName string
param appName string
param acrLoginServer string
param acrName string
@secure()
param cosmosConnectionString string
@secure()
param acrPassword string // We need to add this to main.bicep later!

param imageTag string

// 1. ENVIRONMENT (The Cluster)
resource env 'Microsoft.App/managedEnvironments@2025-07-01' = {
  name: environmentName
  location: location
  properties: {
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

// 2. Container App
resource app 'Microsoft.App/containerApps@2025-07-01' = {
  name: appName
  location: location
  properties: {
    managedEnvironmentId: env.id
    configuration: {
      //INGRESS: Allow traffic from the internet
      ingress: {
        external: true
        //targetPort: 80 // Hello world image uses port 80 (change to 8080 when using your .NET app)
        targetPort: 8080
        transport: 'auto'
      }
      // REGISTRIES: How to log in to ACR
      registries: [
        {
          server: acrLoginServer
          username: acrName
          //The passwordSecretRef field expects a string reference to a secret defined in the secrets array. 
          //It should match the name property of one of your secrets:
          passwordSecretRef: 'acr-password' //not the actual value: acrPassword
        }
      ]
      // SECRETS: Safe storage for sensitive strings
      secrets: [
        {
          name: 'acr-password'
          value: acrPassword
        }
        {
          name: 'cosmos-connection-string'
          value: cosmosConnectionString
        }
      ]
    }
    template: {
      //CONTAINERS: The actual running code
      containers: [
        {
          name: 'main-app'
          // STARTER STRATEGY: Use a public Hello World image initially.
          // Your CI/CD pipeline will overwrite this with your real ACR image later.
          //image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

          // Update the image reference to use the param
          image: '${acrLoginServer}/modern-stoic-app:${imageTag}'

          // RESOURCE ALLOCATION:
          // Since you are running ONNX locally, we need more juice than the minimum.
          // 0.5 CPU / 1.0 GB Memory is a safe starting point for small models.
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }

          // ENVIRONMENT VARIABLES: Injecting secrets into the .NET App
          env: [
            {
              name: 'ConnectionStrings__CosmosDb' // Matches .NET appsettings structure // Still works for NoSQL API
              secretRef: 'cosmos-connection-string'
            }
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Development'
            }
          ]
        }
      ]
    }
  }
}

output fqdn string = app.properties.configuration.ingress.fqdn
