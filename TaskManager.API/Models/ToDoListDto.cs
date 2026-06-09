namespace TaskManager.Models
{
    public class ToDoListDto
    {
        public string Summary { get; private set; }

        public ToDoTaskDto[] Tasks { get; private set; }
    }
}