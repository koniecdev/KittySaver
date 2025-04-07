var builder = DistributedApplication.CreateBuilder(args);

var authApi = builder.AddProject<Projects.KittySaver_Auth_Api>("kittysaver-auth-api");

var api = builder.AddProject<Projects.KittySaver_Api>("kittysaver-api").WithReference(authApi);

builder.AddProject<Projects.KittySaver_Wasm>("kittysaver-wasm").WithReference(api).WithReference(authApi);

builder.Build().Run();
