
using TaskBoard.Infrastructure.Persistence;
using TaskBoard.Application.Services;
using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;
using TaskBoard.Api.Middleware;
using TaskBoard.Api.Security;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using TaskBoard.Api.Filters;
using Microsoft.EntityFrameworkCore;




var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ITime, SystemClock>();
builder.Services.AddSingleton<IGeneratorId, IdGenerator>();

var jsonPath = builder.Configuration["Storage:Json:FilePath"] ?? "App_Data/tasks.json";

/*
var fullJsonPath = Path.IsPathRooted(jsonPath)
    ? jsonPath
    : Path.Combine(builder.Environment.ContentRootPath, jsonPath);

builder.Services.AddSingleton<ITaskRepository>(_ => new JsonRepository(fullJsonPath));
*/

builder.Services.AddDbContext<TaskBoardDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("dbTaskBoard")));
    
builder.Services.AddScoped<ITaskRepository, EFRepository>();


builder.Services.AddScoped<CreateTask>();
builder.Services.AddScoped<DeleteTask>();
builder.Services.AddScoped<Updatetask>();

builder.Services.Configure<KeyOptions>(builder.Configuration.GetSection(KeyOptions.SectionName));
builder.Services.AddTransient<KeyMiddleware>();

//ApiKey nastaveni do swaggeru
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


// Patch Task null filtr
builder.Services.AddSwaggerGen(c =>
{
    c.SchemaFilter<TaskPatchSchemaFilter>();
});



builder.Services
    .AddControllers()
//Kontrola enumu 
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
    });

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/api/admin", StringComparison.OrdinalIgnoreCase),
    branch => branch.UseMiddleware<KeyMiddleware>()
);


app.MapControllers();

app.Run();

