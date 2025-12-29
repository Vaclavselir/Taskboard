using TaskBoard.Application.Services;
using TaskBoard.Domain;
using TaskBoard.Application.Common;
using TaskBoard.Infrastructure.Persistence;
using System.Text.Json.Serialization;




var repo = new JsonRepository(@"G:\tasks.json");
var clock = new SystemClock();
var ids = new IdGenerator();


var create = new CreateTask(repo, clock, ids);
var changeStat = new ChangeStatus(repo);
var list = new ListTask(repo);
var delete = new DeleteTask(repo);
var changePrio = new ChangePriority(repo);
var update = new Updatetask(repo);


create.TaskCreated += t => Console.WriteLine($"[CREATED] {t.Id} | {t.Title} | {t.Priority} | {t.Status}");

changeStat.StatusChanged += (id, oldS, newS) => Console.WriteLine($"[STATUS]  {id} {oldS} -> {newS}");

changePrio.PriorityChanged += (id, oldP, newP) => Console.WriteLine($"[PRIORITY]  {id} {oldP} -> {newP}");

delete.TaskDeleted += t => Console.WriteLine($"[DELETED] {t.Id} | {t.Title} | {t.Priority} | {t.Status}");

update.TaskUpdated += t => Console.WriteLine($"[UPDATE] {t.Id} | {t.Title}");


var id = create.Create(new TaskCommand(

    Title: "Naučit se eventy bez breku",

    Description: "A pak to přetavit do Web API.",

    Priority: Priority.Low,

    DueDate: DateTime.Now.AddDays(3),

    Tags: new[] { "csharp", "bug-123" }

));


changeStat.ChangeSta(id, Status.Doing);




Console.WriteLine();


changePrio.ChangePri(id, Priority.Medium);



Console.WriteLine();

var idTask = Guid.Parse("f3e5c260-ccfb-4b60-9e36-f54e99b2c189");

update.Update(id, "nic moc název", "gg popisek", null);

update.Update(idTask, "ggg", "gg popisek", null);


var filtered = list.List(Status.Done, Priority.High, "csharp");


Console.WriteLine("Filtered tasks:");
foreach (var t in filtered)
    Console.WriteLine($"{t.Id} | {t.Title} | {t.Status} | {t.Priority} | {string.Join(", ", t.Tags)}");


Console.WriteLine();
Console.WriteLine("All tasks:");


foreach (var t in repo.GetAll())
    Console.WriteLine($"{t.Id} | {t.Title} | {t.Status} | {string.Join(", ", t.Tags)}");
    
