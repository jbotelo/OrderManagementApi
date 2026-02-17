using Microsoft.EntityFrameworkCore;
using Orm.Application.Services;
using Orm.Domain.Interfaces;
using Orm.Infrastructure.DbContexts;
using Orm.Infrastructure.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Configure OpenAPI document generation (built-in in .NET 9+)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Order Management API",
            Version = "v1",
            Description = "API for managing customer orders and order items."
        };
        return Task.CompletedTask;
    });
});

builder.Services.AddDbContext<AppDbContext>(optBuilder =>
    optBuilder.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnectionString"))
);
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IMapper, Mapper>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Serve the OpenAPI JSON document at /openapi/v1.json
    app.MapOpenApi();

    // Serve the Scalar interactive UI at /scalar/v1
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
