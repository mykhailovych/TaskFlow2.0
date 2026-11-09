using Microsoft.EntityFrameworkCore;
using TaskFlow.Server.Models;

namespace TaskFlow.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
    }
}
