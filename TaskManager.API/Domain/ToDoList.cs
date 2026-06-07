namespace TaskManager.Domain
{
    public class ToDoList
    {
        public string Summary { get; private set; }

        public ToDoTask[] Tasks { get; private set; }

        public static ToDoList Create(string summary)
        {
            return new ToDoList { Summary = summary, Tasks = Array.Empty<ToDoTask>() };
        }
    }
}
