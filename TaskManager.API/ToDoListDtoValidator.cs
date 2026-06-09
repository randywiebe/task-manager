using TaskManager.Models;

namespace TaskManager.API
{
    public static class ToDoListDtoValidator
    {
        public static Dictionary<string, string[]> IsValid(ToDoListDto dto)
        {
            Dictionary<string, string[]> errors = new();

            if (string.IsNullOrWhiteSpace(dto.Summary))
            {
                errors.TryAdd("list.summary.errors", new[] { "Summary is empty" });
            }

            if (dto.Summary.Length > 50)
            {
                errors.TryAdd("list.summary.errors", new[] { "Summary is too long" });
            }

            return errors;
        }
    }
}
