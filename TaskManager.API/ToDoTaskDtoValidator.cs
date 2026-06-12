using TaskManager.Models;

namespace TaskManager.API
{
    public static class ToDoTaskDtoValidator
    {
        public static Dictionary<string, string[]> IsValid(ToDoTaskDto dto)
        {
            Dictionary<string, string[]> errors = new();

            try
            {

                if (string.IsNullOrWhiteSpace(dto.Summary))
                {
                    errors.TryAdd("list.summary.errors", new[] { "Summary is empty" });
                }

                if (dto.Summary.Length > 50)
                {
                    errors.TryAdd("list.summary.errors", new[] { "Summary is too long" });
                }
            }
            catch (Exception ex)
            {
                // log exception

                // Add an error so the client knows something went wrong
                errors.TryAdd("list.summary.errors", new[] { "Unable to save" });
            }

            return errors;
        }
    }
}
