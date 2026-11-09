namespace TaskFlow.Server.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }


        public int ColumnId { get; set; }
        public Column? Column { get; set; }
        public int? AssigneeId { get; set; }
        public User? Assignee { get; set; }

        public int Order { get; set; }
        public string Priority { get; set; } = "Medium";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
