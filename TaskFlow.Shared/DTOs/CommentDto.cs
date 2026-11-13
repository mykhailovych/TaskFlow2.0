using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Shared.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
