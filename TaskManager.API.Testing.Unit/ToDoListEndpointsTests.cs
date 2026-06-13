using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskManager.Persistence;
using TaskManager.Models;
using TaskManager.API;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TaskManager.API.Testing.Unit
{
    [TestClass]
    public class ToDoListEndpointsTests
    {
        private ToDoListDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ToDoListDbContext>()
                .UseInMemoryDatabase($"TestDb-{Guid.NewGuid()}")
                .Options;
            return new ToDoListDbContext(options);
        }

        [TestMethod]
        public async Task CreateAndGetList_Works()
        {
            await using var db = CreateContext();

            var dto = new ToDoListDto { Summary = "My list", Tasks = new List<ToDoTaskDto> { new ToDoTaskDto { Summary = "t1" } } };

            var createResult = await ToDoListEndpoints.CreateList(dto, db);
            Assert.IsInstanceOfType(createResult.Result, typeof(Created<ToDoList>));

            var created = (Created<ToDoList>)createResult.Result;
            Assert.IsNotNull(created.Value);

            var all = await ToDoListEndpoints.GetAllLists(db);
            Assert.IsInstanceOfType(all.Result, typeof(Ok<ToDoListSummaryResponseModel[]>));

            var get = await ToDoListEndpoints.GetList(created.Value.Id, db);
            Assert.IsInstanceOfType(get.Result, typeof(Ok<ToDoList>));
        }

        [TestMethod]
        public async Task CreateTask_Update_And_Delete_Works()
        {
            await using var db = CreateContext();

            var dto = new ToDoListDto { Summary = "List A" };
            var createList = await ToDoListEndpoints.CreateList(dto, db);
            var created = (Created<ToDoList>)createList.Result;

            // create task
            var taskDto = new ToDoTaskDto { Summary = "task1", Complete = false };
            var createTask = await ToDoListEndpoints.CreateListTask(created.Value.Id, taskDto, db);
            Assert.IsInstanceOfType(createTask.Result, typeof(Created<ToDoTask>));
            var createdTask = (Created<ToDoTask>)createTask.Result;

            // update task
            var updateDto = new ToDoTaskDto { Summary = "task1-updated", Complete = true };
            var updateTask = await ToDoListEndpoints.UpdateTask(created.Value.Id, createdTask.Value.Id, updateDto, db);
            Assert.IsInstanceOfType(updateTask.Result, typeof(Ok));

            // verify task updated
            var get = await ToDoListEndpoints.GetList(created.Value.Id, db);
            var ok = (Ok<ToDoList>)get.Result;
            var list = ok.Value;
            Assert.AreEqual(1, list.Tasks.Count);
            Assert.AreEqual("task1-updated", list.Tasks[0].Summary);
            Assert.IsTrue(list.Tasks[0].Complete);

            // delete task
            var deleteTask = await ToDoListEndpoints.DeleteTask(created.Value.Id, createdTask.Value.Id, db);
            Assert.IsInstanceOfType(deleteTask.Result, typeof(NoContent));

            // delete list
            var deleteList = await ToDoListEndpoints.DeleteList(created.Value.Id, db);
            Assert.IsInstanceOfType(deleteList.Result, typeof(NoContent));
        }

        [TestMethod]
        public async Task GetListTasks_ReturnsNotFound_ForMissingList()
        {
            await using var db = CreateContext();

            var res = await ToDoListEndpoints.GetListTasks(999, db);
            Assert.IsInstanceOfType(res.Result, typeof(NotFound));
        }
    }
}
