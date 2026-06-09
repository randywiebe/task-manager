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
            group.MapPut("/{id}", UpdateList);
            group.MapDelete("/{id}", DeleteList);

            return group;
        }

        public static async Task<Ok<ToDoList[]>> GetAllLists(ToDoListDbContext dbContext)
        {
            var toDoLists = await dbContext.ToDoLists.ToArrayAsync();
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

        public static async Task<Created<ToDoList>> CreateList(ToDoListDto dto, ToDoListDbContext dbContext)
        {
            var newList = new ToDoList
            {
                Summary = dto.Summary
            };

            await dbContext.ToDoLists.AddAsync(newList);
            await dbContext.SaveChangesAsync();

            return TypedResults.Created($"/lists/{newList.Id}", newList);
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
