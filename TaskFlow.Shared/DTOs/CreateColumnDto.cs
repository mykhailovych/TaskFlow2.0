using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Shared.DTOs
{
    public class CreateColumnDto
    {
        public string Name { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public int Order { get; set; }
    }
}
