using TaskManager.API.Persistence;

namespace TaskManager.API
{
    public class ToDoListSummaryResponseModel
    {
        public ToDoListSummaryResponseModel(ToDoList toDoList)
        {
            Id = toDoList.Id;
            Summary = toDoList.Summary;
        }

        public int Id { get; set; }
        public string Summary { get; set; }
    }
}
