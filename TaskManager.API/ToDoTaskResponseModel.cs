using TaskManager.API.Persistence;

namespace TaskManager.API
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
