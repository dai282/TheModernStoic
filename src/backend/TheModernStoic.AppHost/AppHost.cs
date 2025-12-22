var builder = DistributedApplication.CreateBuilder(args);

//load secrets
var cosmosConnection = builder.Configuration["CosmosDb:ConnectionString"];
var hfApiKey = builder.Configuration["AI:HuggingFaceApiKey"];

//Define the API service
builder.AddProject<Projects.TheModernStoic_API>("api")
    .WithEnvironment("ConnectionStrings__CosmosDb", cosmosConnection)
    .WithEnvironment("HuggingFace__ApiKey", hfApiKey)
    .WithExternalHttpEndpoints();

builder.Build().Run();
