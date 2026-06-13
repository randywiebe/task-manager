using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TaskManager.API;
using TaskManager.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// in-memory database - replace this with SQLite or other implementation
builder.Services.AddDbContext<ToDoListDbContext>(options => options.UseInMemoryDatabase("items"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapGroup("/lists")
    .MapListApi()
    .WithTags("List Endpoints");

app.Run();
