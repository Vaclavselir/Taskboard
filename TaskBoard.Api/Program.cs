
using TaskBoard.Domain;
using TaskBoard.Infrastructure.Persistence;
using TaskBoard.Application.Services;
using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ITime, SystemClock>();
builder.Services.AddSingleton<IGeneratorId, IdGenerator>();

builder.Services.AddSingleton<ITaskRepository>(_ => new JsonRepository(@"G:\tasks.json"));

builder.Services.AddScoped<CreateTask>();
builder.Services.AddScoped<ChangePriority>();
builder.Services.AddScoped<ChangeStatus>();

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

