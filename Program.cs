using HospitalApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("HospitalDatabase")
    ?? throw new InvalidOperationException("Connection string 'HospitalDatabase' is missing.");

builder.Services.AddControllers();
builder.Services.AddDbContext<HospitalContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
