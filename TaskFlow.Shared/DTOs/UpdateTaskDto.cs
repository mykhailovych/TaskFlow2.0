using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Shared.DTOs
{
    public class UpdateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ColumnId { get; set; }
        public int Order { get; set; }
        public string Priority { get; set; } = "Medium";
    }
}
