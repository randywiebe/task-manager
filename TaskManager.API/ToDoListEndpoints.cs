using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Models;
using TaskManager.Application;
using TaskManager.Models;
using TaskManager.Persistence;

namespace TaskManager.API
{
    public static class ToDoListEndpoints
    {
        public static RouteGroupBuilder MapListApi(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllLists);
            group.MapGet("/{id}", GetList);
            group.MapGet("/{id}/tasks", GetListTasks);

            group.MapPost("/", CreateList)
                .AddEndpointFilter(async (efiContext, next) =>
                {
                    var param = efiContext.GetArgument<ToDoListDto>(0);

                    var validationErrors = ToDoListDtoValidator.IsValid(param);

                    if (validationErrors.Any())
                    {
                        return Results.ValidationProblem(validationErrors);
                    }

                    return await next(efiContext);
                });

            group.MapPost("/{id}/tasks", CreateListTask)
                .AddEndpointFilter(async (efiContext, next) =>
                {
                    var param = efiContext.GetArgument<ToDoTaskDto>(1);

                    var validationErrors = ToDoTaskDtoValidator.IsValid(param);

                    if (validationErrors.Any())
                    {
                        return Results.ValidationProblem(validationErrors);
                    }

                    return await next(efiContext);
                });

            group.MapPut("/{id}", UpdateList);
            group.MapPut("/{id}/tasks/{taskId}", UpdateTask);

            group.MapDelete("/{id}", DeleteList);
            group.MapDelete("/{id}/tasks/{taskId}", DeleteTask);

            return group;
        }

        public static async Task<Results<Ok<ToDoListSummaryResponseModel[]>, InternalServerError>> GetAllLists(ToDoListDbContext dbContext)
        {
            try
            {
                var toDoLists = await dbContext.ToDoLists.Select(x => new ToDoListSummaryResponseModel(x)).ToArrayAsync();
                return TypedResults.Ok(toDoLists);
            }
            catch (Exception e) when (e is ArgumentNullException || e is OperationCanceledException)
            {
                // TODO: log the error coming from ToArrayAsync
                return TypedResults.InternalServerError();
            }
        }

        public static async Task<Results<Ok<ToDoList>, NotFound, InternalServerError>> GetList(int id, ToDoListDbContext dbContext)
        {
            try
            {
                var toDoList = (await dbContext.ToDoLists.Include(list => list.Tasks).ToListAsync()).FirstOrDefault(x => x.Id == id);

                if (toDoList != null)
                {
                    return TypedResults.Ok(toDoList);
                }

                return TypedResults.NotFound();
            }
            catch (Exception e) when (e is ArgumentNullException || e is OperationCanceledException)
            {
                // TODO: log the error coming from .Include|.ToListAsync|.FirstOrDefault
                return TypedResults.InternalServerError();
            }
        }

        public static async Task<Results<Ok<ToDoTaskResponseModel[]>, NotFound, InternalServerError>> GetListTasks(int id, ToDoListDbContext dbContext)        
        {
            try
            {
                var toDoList = (await dbContext.ToDoLists.Include(list => list.Tasks).ToListAsync()).FirstOrDefault(x => x.Id == id);

                if (toDoList != null)
                {
                    return TypedResults.Ok(toDoList.Tasks.Select(x => new ToDoTaskResponseModel(x)).ToArray());
                }

                return TypedResults.NotFound();
            }
            catch (Exception e) when (e is ArgumentNullException || e is OperationCanceledException)
            {
                // TODO: log the error coming from .Include|.ToListAsync|.FirstOrDefault
                return TypedResults.InternalServerError();
            }
        }

        public static async Task<Results<Created<ToDoList>, InternalServerError>> CreateList(ToDoListDto dto, ToDoListDbContext dbContext)
        {
            try
            {
                var newList = new ToDoList
                {
                    Summary = dto.Summary,
                    Tasks = dto.Tasks.Select(x =>
                        new ToDoTask
                        {
                            Summary = x.Summary,
                            Complete = x.Complete
                        }
                    ).ToList()
                };

                dbContext.ToDoLists.Add(newList);
                dbContext.ToDoLists.Include(l => l.Tasks);
                await dbContext.SaveChangesAsync();

                return TypedResults.Created($"/lists/{newList.Id}", newList);
            }
            catch (Exception e) when (e is ArgumentNullException || e is DbUpdateException || e is DbUpdateConcurrencyException || e is OperationCanceledException)
            {
                // TODO: log the error coming from .Select|.ToList|.Include|.SaveChangesAsync
                return TypedResults.InternalServerError();
            }
        }

        public static async Task<Results<Created<ToDoTask>, NotFound, InternalServerError>> CreateListTask(int id, ToDoTaskDto dto, ToDoListDbContext dbContext)
        {
            try
            {
                var list = (await dbContext.ToDoLists.Include(list => list.Tasks).ToListAsync()).FirstOrDefault(x => x.Id == id);

                if (list == null)
                {
                    return TypedResults.NotFound();
                }

                var newTask = new ToDoTask
                {
                    Summary = dto.Summary,
                    Complete = dto.Complete
                };

                list.Tasks.Add(newTask);

                await dbContext.SaveChangesAsync();

                return TypedResults.Created($"/lists/{id}/tasks/{newTask.Id}", newTask);
            }
            catch (Exception e) when (e is ArgumentNullException || e is DbUpdateException || e is DbUpdateConcurrencyException || e is OperationCanceledException)
            {
                // TODO: log the error coming from .Include|.ToListAsync|FirstOrDefault|.SaveChangesAsync
                return TypedResults.InternalServerError();
            }
        }

        public static async Task<Results<Ok, NotFound, InternalServerError>> UpdateList(int id, ToDoListDto listDto, ToDoListDbContext dbContext)
        {
            try
            {
                var existingList = (await dbContext.ToDoLists.Include(list => list.Tasks).ToListAsync()).FirstOrDefault(x => x.Id == id);

                if (existingList != null)
                {
                    existingList.Summary = listDto.Summary;
                    existingList.Tasks = listDto.Tasks.Select(x => new ToDoTask { Summary = x.Summary, Complete = x.Complete }).ToList();

                    await dbContext.SaveChangesAsync();

                    return TypedResults.Ok();
                }

                return TypedResults.NotFound();
            }
            catch (Exception e) when (e is ArgumentNullException || e is DbUpdateException || e is DbUpdateConcurrencyException || e is OperationCanceledException)
            {
                // TODO: log the error coming from .Include|.ToListAsync|FirstOrDefault|.Select|.ToList|.SaveChangesAsync
                return TypedResults.InternalServerError();
            }
        }

        public static async Task<Results<Ok, NotFound, InternalServerError>> UpdateTask(int id, int taskId, ToDoTaskDto taskDto, ToDoListDbContext dbContext)
        {
            try
            {
                var list = (await dbContext.ToDoLists.Include(list => list.Tasks).ToListAsync()).FirstOrDefault(x => x.Id == id);

                if (list != null)
                {
                    var task = list.Tasks.FirstOrDefault(t => t.Id == taskId);

                    if (task != null)
                    {
                        task.Summary = taskDto.Summary;
                        task.Complete = taskDto.Complete;
                    }

                    await dbContext.SaveChangesAsync();

                    return TypedResults.Ok();
                }

                return TypedResults.NotFound();
            }
            catch (Exception e) when (e is ArgumentNullException || e is DbUpdateException || e is DbUpdateConcurrencyException || e is OperationCanceledException)
            {
                // TODO: log the error coming from .Include|.ToListAsync|FirstOrDefault|.SaveChangesAsync
                return TypedResults.InternalServerError();
            }
        }

        public static async Task<Results<NoContent, NotFound, InternalServerError>> DeleteList(int id, ToDoListDbContext dbContext)
        {
            try
            {
                var list = (await dbContext.ToDoLists.Include(list => list.Tasks).ToListAsync()).FirstOrDefault(x => x.Id == id);

                if (list != null)
                {
                    dbContext.ToDoLists.Remove(list);
                    await dbContext.SaveChangesAsync();

                    return TypedResults.NoContent();
                }

                return TypedResults.NotFound();
            }
            catch (Exception e) when (e is ArgumentNullException || e is DbUpdateException || e is DbUpdateConcurrencyException || e is OperationCanceledException)
            {
                // TODO: log the error coming from .Include|.ToListAsync|.FirstOrDefault|.SaveChangesAsync
                return TypedResults.InternalServerError();
            }
        }

        public static async Task<Results<NoContent, NotFound, InternalServerError>> DeleteTask(int id, int taskId, ToDoListDbContext dbContext)
        {
            try
            {
                var list = (await dbContext.ToDoLists.Include(list => list.Tasks).ToListAsync()).FirstOrDefault(x => x.Id == id);

                if (list != null)
                {
                    var task = list.Tasks.FirstOrDefault(t => t.Id == taskId);
                    if (task != null)
                    {
                        list.Tasks.Remove(task);
                        await dbContext.SaveChangesAsync();

                        return TypedResults.NoContent();
                    }
                }

                return TypedResults.NotFound();
            }
            catch (Exception e) when (e is DbUpdateException || e is DbUpdateConcurrencyException || e is OperationCanceledException)
            {
                // TODO: log the error coming from .Include|.ToListAsync|.FirstOrDefault|.SaveChangesAsync
                return TypedResults.InternalServerError();
            }
        }
    }
}
