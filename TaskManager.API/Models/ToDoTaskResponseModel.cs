using TaskManager.Persistence;

namespace TaskManager.API.Models
{
    public class ToDoTaskResponseModel
    {
        public ToDoTaskResponseModel(ToDoTask task)
        {
            Id = task.Id;
            Summary = task.Summary;
            Complete = task.Complete;
        }

        public int Id { get; set; }
        public string Summary { get; set; }
        public bool Complete { get; set; }
    }
}
