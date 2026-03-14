using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskBoard.Application.Abstractions;
using TaskBoard.Infrastructure.Persistence;

namespace TaskBoard.Infrastructure;

public static class DependencyInjection
{

    public static IServiceCollection AddTaskBoardStorage(this IServiceCollection services, IConfiguration config, string contentRootPath)
    {

        string? provider = config["Storage:Provider"]?.Trim();

        if (string.Equals(provider, "Json", StringComparison.OrdinalIgnoreCase))
        {

            string jsonPath = config["Storage:Json:FilePath"] ?? "App_Data/tasks.json";

            string fullPath = Path.IsPathRooted(jsonPath)
                ? jsonPath
                : Path.Combine(contentRootPath, jsonPath);

            services.AddSingleton<JsonRepository>(_ => new JsonRepository(fullPath));
            services.AddSingleton<ITaskRepository>(sp => sp.GetRequiredService<JsonRepository>());
            services.AddSingleton<IUserRepository>(sp => sp.GetRequiredService<JsonRepository>());

            return services;

        }

        services.AddDbContext<TaskBoardDbContext>(o =>
            o.UseSqlServer(config.GetConnectionString("dbTaskBoard")));

        services.AddScoped<EFRepository>();
        services.AddScoped<ITaskRepository>(sp => sp.GetRequiredService<EFRepository>());
        services.AddScoped<IUserRepository>(sp => sp.GetRequiredService<EFRepository>());

        return services;

    }

}
