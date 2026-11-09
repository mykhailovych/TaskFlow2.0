using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Shared.DTOs
{
    public class ColumnDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public int ProjectId { get; set; }
        public List<TaskDto> Tasks { get; set; } = new();
    }
}
