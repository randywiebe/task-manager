using Microsoft.AspNetCore.Http.HttpResults;
using TaskManager.Persistence;

namespace TaskManager.API.Testing.Unit
{
    [TestClass]
    public sealed class ToDoListsInMemoryTests
    {
        [TestMethod]
        public async Task GetToDoList_ReturnsNotFound_IfNotExists()
        {
            // Arrange
            await using var context = new MockDb().CreateDbContext();

            // Act
            var result = await ToDoListEndpoints.GetList(1, context);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFound));

            var notFoundResult = (NotFound)result.Result;

            Assert.IsNotNull(notFoundResult);
        }
    }
}
