namespace TaskManager.Persistence
{
    public class ToDoTask
    {
        public int Id { get; set; }
        public string Summary { get; set; } = string.Empty;
        public bool Complete { get; set; }
    }
}
