using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Persistence;
using TaskManager.Models;

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
                    var param = efiContext.GetArgument<ToDoTaskDto>(0);

                    var validationErrors = ToDoTaskDtoValidator.IsValid(param);

                    if (validationErrors.Any())
                    {
                        return Results.ValidationProblem(validationErrors);
                    }

                    return await next(efiContext);
                });

            group.MapPut("/{id}", UpdateList);
            group.MapDelete("/{id}", DeleteList);

            return group;
        }

        public static async Task<Ok<ToDoListSummaryResponseModel[]>> GetAllLists(ToDoListDbContext dbContext)
        {
            var toDoLists = await dbContext.ToDoLists.Select(x => new ToDoListSummaryResponseModel(x)).ToArrayAsync();
            return TypedResults.Ok(toDoLists);
        }

        public static async Task<Results<Ok<ToDoList>, NotFound>> GetList(int id, ToDoListDbContext dbContext)
        {
            var toDoList = await dbContext.ToDoLists.FindAsync(id);

            if (toDoList != null)
            {
                return TypedResults.Ok(toDoList);
            }

            return TypedResults.NotFound();
        }

        public static async Task<Results<Ok<ToDoTaskResponseModel[]>, NotFound>> GetListTasks(int id, ToDoListDbContext dbContext)        
        {
            var toDoList = (await dbContext.ToDoLists.Include(list => list.Tasks).ToListAsync()).FirstOrDefault(x => x.Id == id);

            if (toDoList != null)
            {
                return TypedResults.Ok(toDoList.Tasks.Select(x => new ToDoTaskResponseModel(x)).ToArray());
            }

            return TypedResults.NotFound();
        }

        public static async Task<Created<ToDoList>> CreateList(ToDoListDto dto, ToDoListDbContext dbContext)
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

        public static async Task<Results<Created<ToDoTask>, NotFound>> CreateListTask(int id, ToDoTaskDto dto, ToDoListDbContext dbContext)
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

        public static async Task<Results<NoContent, NotFound>> DeleteList(int id, ToDoListDbContext dbContext)
        {
            var list = await dbContext.ToDoLists.FindAsync(id);

            if (list != null)
            {
                dbContext.ToDoLists.Remove(list);
                await dbContext.SaveChangesAsync();

                return TypedResults.NoContent();
            }

            return TypedResults.NotFound();
        }

        public static async Task<Results<Created<ToDoList>, NotFound>> UpdateList(ToDoList list, ToDoListDbContext dbContext)
        {
            var existingList = await dbContext.ToDoLists.FindAsync(list.Id);

            if (existingList != null)
            {
                existingList.Summary = list.Summary;

                await dbContext.SaveChangesAsync();

                return TypedResults.Created($"/lists/{existingList.Id}", existingList);
            }

            return TypedResults.NotFound();
        }
    }
}
