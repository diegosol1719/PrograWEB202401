using BackEnd.Services.Implementations;
using BackEnd.Services.Interfaces;
using DAL.Implementations;
using DAL.Interfaces;
using Entities.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext< QuizContext>();
builder.Services.AddScoped<IUnidadDeTrabajo, IUnidadDeTrabajo>();
builder.Services.AddScoped<IEmpleadoDAL, EmpleadoDALImpl>();            
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();    
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
