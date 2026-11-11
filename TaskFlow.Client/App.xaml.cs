using System.Configuration;
using System.Data;
using System.Windows;
using TaskFlow.Shared.DTOs;

namespace TaskFlow.Client
{
    
    public partial class App : Application
    {
        public static UserDto? CurrentUser { get; set; }
    }

}
