using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Shared.DTOs
{
    public class UpdateColumnDto
    {
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }

    }
}
