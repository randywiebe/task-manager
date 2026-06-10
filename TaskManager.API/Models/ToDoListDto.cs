namespace TaskManager.Models
{
    public class ToDoListDto
    {
        public string Summary { get; set; }

        public ToDoTaskDto[] Tasks { get; set; }
    }
}