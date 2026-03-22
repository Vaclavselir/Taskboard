using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;

namespace TaskBoard.Api.Services;

public sealed class TaskStatisticsService : BackgroundService
{

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TaskStatisticsService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public TaskStatisticsService(IServiceScopeFactory scopeFactory, ILogger<TaskStatisticsService> logger)
    {

        _scopeFactory = scopeFactory;

        _logger = logger;

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        _logger.LogInformation("TaskStatisticsService started. Interval: {Interval}", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {

            try
            {

                RunCheck();

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error in TaskStatisticsService");

            }

            await Task.Delay(_interval, stoppingToken);

        }

    }

    private void RunCheck()
    {

        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

        var allTasks = repo.GetAllTracked();

        foreach (var task in allTasks)
        {

            task.MarkChecked();

        }

        repo.Save();

        var total = allTasks.Count;
        var overdue = allTasks.Count(t => t.IsOverdue);
        var todo = allTasks.Count(t => t.Status == Status.Todo);
        var doing = allTasks.Count(t => t.Status == Status.Doing);
        var done = allTasks.Count(t => t.Status == Status.Done);

        _logger.LogInformation(

            "[Stats] Total: {Total} | Todo: {Todo} | Doing: {Doing} | Done: {Done} | Overdue: {Overdue}",
            total, todo, doing, done, overdue

        );

    }
}
