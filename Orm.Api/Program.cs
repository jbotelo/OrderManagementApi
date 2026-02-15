using Microsoft.EntityFrameworkCore;
using Orm.Application.Services;
using Orm.Domain.Interfaces;
using Orm.Infrastructure.DbContexts;
using Orm.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
