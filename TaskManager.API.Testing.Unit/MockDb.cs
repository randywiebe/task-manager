using Microsoft.EntityFrameworkCore;
using TaskManager.API.Persistence;

namespace TaskManager.API.Testing.Unit
{
    internal class MockDb : IDbContextFactory<ToDoListDbContext>
    {
        public ToDoListDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ToDoListDbContext>()
                .UseInMemoryDatabase($"InMemoryTestDb-{DateTime.Now.ToFileTimeUtc()}")
                .Options;

            return new ToDoListDbContext(options);
        }
    }
}
