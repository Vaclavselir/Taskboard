
using TaskBoard.Domain;
using TaskBoard.Infrastructure.Persistence;
using TaskBoard.Application.Services;
using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;
using TaskBoard.Api.Middleware;
using TaskBoard.Api.Security;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;



var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ITime, SystemClock>();
builder.Services.AddSingleton<IGeneratorId, IdGenerator>();

builder.Services.AddSingleton<ITaskRepository>(_ => new JsonRepository(@"G:\tasks.json"));

builder.Services.AddScoped<CreateTask>();
builder.Services.AddScoped<DeleteTask>();
builder.Services.AddScoped<Updatetask>();

builder.Services.Configure<KeyOptions>(builder.Configuration.GetSection(KeyOptions.SectionName));
builder.Services.AddTransient<KeyMiddleware>();


builder.Services.AddSwaggerGen(c =>
{

    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskBoard API", Version = "v1" });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {

        Type = SecuritySchemeType.ApiKey,
        Name = "X-API-KEY",
        In = ParameterLocation.Header,
        Description = "API Key needed to access /api/* endpoints"

    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {

        {

            new OpenApiSecurityScheme
            {

                Reference = new OpenApiReference
                {

                    Type = ReferenceType.SecurityScheme,

                    Id = "ApiKey"

                }

            },
            Array.Empty<string>()
            
        }

    });

});


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

app.UseMiddleware<ExceptionMiddleware>();

app.UseMiddleware<KeyMiddleware>();


app.MapControllers();

app.Run();

