namespace TaskManager.Domain
{
    /// <summary>
    /// Individual tasks making up a to-do list.
    /// Sadly the name `Task` was already taken...
    /// </summary>
    public class ToDoTask
    {
        public string Summary { get; private set; }
        public bool Complete { get; private set; }
        public DateTimeOffset? DueDate { get; private set; }

        public static ToDoTask Create(string summary)
        {
            return new ToDoTask { Summary = summary };
        }

        public static ToDoTask Create(string summary, DateTimeOffset dueDate)
        {
            return new ToDoTask { Summary = summary, DueDate = dueDate };
        }
    }
}
