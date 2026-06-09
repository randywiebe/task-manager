using Microsoft.EntityFrameworkCore;

namespace TaskManager.API.Persistence
{
    public class ToDoListDbContext : DbContext
    {
        public ToDoListDbContext(DbContextOptions options) : base(options) { }
        public DbSet<ToDoList> ToDoLists { get; set; } = null!;
    }
}
