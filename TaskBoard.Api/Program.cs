using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TaskBoard.Application.Services;
using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;
using TaskBoard.Api.Middleware;
using TaskBoard.Api.Security;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using TaskBoard.Api.Filters;
using TaskBoard.Infrastructure;
using TaskBoard.Infrastructure.Security;
using TaskBoard.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
//Kontrola enumu 
    .AddJsonOptions(o =>
    {

        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<ITime, SystemClock>();
builder.Services.AddSingleton<IGeneratorId, IdGenerator>();

builder.Services.AddTaskBoardStorage(builder.Configuration, builder.Environment.ContentRootPath);

builder.Services.AddScoped<CreateTask>();
builder.Services.AddScoped<DeleteTask>();
builder.Services.AddScoped<Updatetask>();
builder.Services.AddScoped<AdminSeeder>();
builder.Services.AddScoped<IAuth, AuthService>();
builder.Services.AddScoped<IJwtToken, JwtTokenService>();
builder.Services.AddScoped<IHasher, HasherAdapter>();


builder.Services.Configure<KeyOptions>(builder.Configuration.GetSection(KeyOptions.SectionName));

builder.Services.AddTransient<KeyMiddleware>();

// Jwt authentication
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero

        };

    });

builder.Services.AddAuthorization();
builder.Services.AddHostedService<TaskStatisticsService>();

//Prihlasovani a nastaveni do swaggeru
builder.Services.AddSwaggerGen(c =>
{

    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskBoard API", Version = "v1" });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {

        Type = SecuritySchemeType.ApiKey,
        Name = "X-API-KEY",
        In = ParameterLocation.Header,
        Description = "Admin API key"

    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {

        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Zadej token ve tvaru: Bearer {token}"

    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {

        {

            new OpenApiSecurityScheme
            {

                Reference = new OpenApiReference
                {

                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"

                }

            },

            Array.Empty<string>()

        }

    });


    c.OperationFilter<AdminApiKey>();

    c.SchemaFilter<TaskPatchSchemaFilter>();

});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{

    var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();

    await seeder.SeedAsync();

}

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

