using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Shared.DTOs
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ColumnId { get; set; }
        public string Priority { get; set; } = "Medium";
    }
}
