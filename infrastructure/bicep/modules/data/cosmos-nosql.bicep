// Azure Cosmos DB NoSQL API with Free Tier

// 1. PARAMETERS
param location string
param accountName string

// 2. RESOURCE: Cosmos DB Account (NoSQL API)
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2025-10-15' = {
  name: accountName
  location: location
  kind: 'GlobalDocumentDB' // NoSQL API
  properties: {
    databaseAccountOfferType: 'Standard'
    enableFreeTier: true // FREE TIER!
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: [
      {
        name: 'EnableNoSQLVectorSearch' // Vector search support
      }
    ]
    capacity: {
      totalThroughputLimit: 1000 // Free tier limit
    }
  }
}

// 3. RESOURCE: Database
resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2025-10-15' = {
  parent: cosmosAccount
  name: 'ModernStoicDB'
  properties: {
    resource: {
      id: 'ModernStoicDB'
    }
  }
}

// 4. RESOURCE: Container with vector search
resource container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2025-10-15' = {
  parent: database
  name: 'Entries'
  properties: {
    resource: {
      id: 'Entries'
      partitionKey: {
        paths: ['/userId']
        kind: 'Hash'
      }
      indexingPolicy: {
        automatic: true
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
        vectorIndexes: [
          {
            path: '/embedding'
            type: 'diskANN' // DiskANN for vector search
          }
        ]
      }
      vectorEmbeddingPolicy: {
        vectorEmbeddings: [
          {
            path: '/embedding'
            dataType: 'float32'
            dimensions: 1536 // OpenAI ada-002 dimension, adjust if needed
            distanceFunction: 'cosine'
          }
        ]
      }
    }
    options: {
      throughput: 400 // Minimum for free tier
    }
  }
}

// 5. OUTPUTS
// Connection string format for NoSQL API
@secure()
output connectionString string = cosmosAccount.listConnectionStrings().connectionStrings[0].connectionString
output endpoint string = cosmosAccount.properties.documentEndpoint
@secure()
output primaryKey string = cosmosAccount.listKeys().primaryMasterKey
