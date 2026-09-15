using WalletTransfer.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException(
        "A string de conexão 'Database' não foi configurada.");

var externalServicesBaseUrl =
    builder.Configuration["ExternalServices:BaseUrl"]
    ?? throw new InvalidOperationException(
        "A URL dos serviços externos não foi configurada.");

builder.Services.AddInfrastructure(
    connectionString,
    externalServicesBaseUrl);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();