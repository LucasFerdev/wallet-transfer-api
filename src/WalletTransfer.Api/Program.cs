using WalletTransfer.Infrastructure.Persistence;
using WalletTransfer.Api.Common.Exceptions;
using System.Text.Json.Serialization;

using WalletTransfer.Application;
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

builder.Services.AddApplication();

builder.Services.AddInfrastructure(
    connectionString,
    externalServicesBaseUrl);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();