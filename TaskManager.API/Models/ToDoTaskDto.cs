namespace TaskManager.Models
{
    /// <summary>
    /// Individual tasks making up a to-do list.
    /// Sadly the name `Task` was already taken...
    /// </summary>
    public class ToDoTaskDto
    {
        public string Summary { get; private set; }
        public bool Complete { get; private set; }
        public DateTimeOffset? DueDate { get; private set; }
    }
}
