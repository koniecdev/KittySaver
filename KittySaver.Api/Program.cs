using KittySaver.Api.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerServices();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.AddSwagger();
}

app.UseHttpsRedirection();

app.Run();