using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Shared.DTOs
{
    public class CreateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OwnerId { get; set; }
    }
}
