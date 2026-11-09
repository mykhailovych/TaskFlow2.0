namespace TaskFlow.Server.Models
{
    public class Column
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public int Order { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public List<TaskItem> Tasks { get; set; } = new();
    }

    
}
