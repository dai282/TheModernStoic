//Defines Azure Container App environement and the app itself
param location string
param environmentName string
param appName string
param dockerHubUsername string
@secure()
param cosmosConnectionString string
@secure()
param huggingFaceApiKey string
param imageTag string
@secure()
param auth0Domain string
@secure()
param auth0Audience string

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
      // SECRETS: Safe storage for sensitive strings
      secrets: [
        {
          name: 'cosmos-connection-string'
          value: cosmosConnectionString
        }
        {
          name: 'hf-api-key' // Internal secret name
          value: huggingFaceApiKey
        }
        {
          name: 'auth0-domain'
          value: auth0Domain
        }
        {
          name: 'auth0-audience'
          value: auth0Audience
        }
      ]
    }
    template: {
      //CONTAINERS: The actual running code
      containers: [
        {
          name: 'main-app'
          // Image from Docker Hub
          image: '${dockerHubUsername}/modern-stoic-app:${imageTag}'

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
              name: 'ConnectionStrings__CosmosDb'
              secretRef: 'cosmos-connection-string'
            }
            {
              // Maps to builder.Configuration["AI:HuggingFaceApiKey"]
              name: 'AI__HuggingFaceApiKey' 
              secretRef: 'hf-api-key'
            }
            {
              // Hardcode or add param for ModelId if needed
              name: 'HuggingFace__ModelId'
              value: 'meta-llama/Llama-3.1-8B-Instruct' 
            }
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Development'
            }
            {
              name: 'Auth0__Domain'
              secretRef: 'auth0-domain'
            }
            {
              name: 'Auth0__Audience'
              secretRef: 'auth0-audience'
            }
          ]
        }
      ]
    }
  }
}

output fqdn string = app.properties.configuration.ingress.fqdn
