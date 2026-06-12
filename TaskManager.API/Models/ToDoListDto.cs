namespace TaskManager.Models
{
    public class ToDoListDto
    {
        public string Summary { get; set; } = string.Empty;

        public List<ToDoTaskDto> Tasks { get; set; } = new List<ToDoTaskDto>();
    }
}