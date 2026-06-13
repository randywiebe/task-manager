namespace TaskManager.Persistence
{
    public class ToDoList
    {
        public int Id { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<ToDoTask> Tasks { get; set; } = new List<ToDoTask>();
    }
}
