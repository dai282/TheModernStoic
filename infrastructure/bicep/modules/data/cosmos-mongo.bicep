//database

// 1. PARAMETERS
param location string
param accountName string
param adminLogin string = 'monGoAdmin' // Hardcoded default for simplicity

@secure()
param adminPassword string = 'P@ssw0rd1234!' // TEMPORARY! We should generate the password using the GitHub Actions secrets

// 2. RESOURCE: The vCore Cluster

resource mongoCluster 'Microsoft.DocumentDB/mongoClusters@2025-09-01' = {
  name: accountName
  location: location
  properties: {
    administrator: {
      userName: adminLogin
      password: adminPassword
    }
    compute: {
      tier: 'Free'
    }
    //serverVersion: '' defaults to latest version
    storage: {
      sizeGb: 32 // Free tier allows 32GB
    }
    sharding: {
      shardCount: 1 // Free tier is limited to 1 shard
    }
    highAvailability: {
      targetMode: 'Disabled'
    }
    createMode: 'Default'
    publicNetworkAccess: 'Enabled'
  }
}

// 2. RESOURCE: Firewall Rule
resource allowAll 'Microsoft.DocumentDB/mongoClusters/firewallRules@2025-09-01' = {
  parent: mongoCluster
  name: 'allowAll'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '255.255.255.255'
  }
}

// 3. OUTPUTS
// We manually construct the connection string because the secure connection string 
//isn't always exposed directly as a clean output.
// Note: vCore uses the 'mongocluster.cosmos.azure.com' domain.
output connectionString string = 'mongodb+srv://${adminLogin}:${adminPassword}@${accountName}.mongocluster.cosmos.azure.com/?tls=true&authMechanism=SCRAM-SHA-256&retrywrites=false&maxIdleTimeMS=120000'
