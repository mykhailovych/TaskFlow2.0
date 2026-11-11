using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using TaskFlow.Client.Services;
using TaskFlow.Shared.DTOs;
using System.Threading.Tasks;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace TaskFlow.Client.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string _userNameDisplay = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ColumnDto> _columns = new();

        public MainViewModel()
        {
            _apiService = new ApiService();

            if(App.CurrentUser != null)
            {
                UserNameDisplay = $"Користувач: {App.CurrentUser.Username}";

            }
        }

        public async Task LoadBoardAsync()
        {
            if (App.CurrentUser == null) return;

            // 1. Отримуємо проєкти користувача
            var projects = await _apiService.GetUserProjectsAsync(App.CurrentUser.Id);

            ProjectDto? currentProject = null;

            // 2. Якщо проєктів немає — створюємо базовий автоматично
            if (!projects.Any())
            {
                var newProjectDto = new CreateProjectDto
                {
                    Name = "Моя перша дошка",
                    Description = "Стартовий проєкт",
                    OwnerId = App.CurrentUser.Id
                };

                currentProject = await _apiService.CreateProjectAsync(newProjectDto);
            }
            else
            {
                currentProject = projects.First();
            }

            // 3. Якщо проєкт є (знайшли або щойно створили) — завантажуємо колонки
            if (currentProject != null)
            {
                var board = await _apiService.GetProjectBoardAsync(currentProject.Id);

                Columns.Clear();
                foreach (var col in board)
                {
                    Columns.Add(col);
                }
            }
        }

        [RelayCommand]
        private async Task AddTaskAsync(ColumnDto column)
        {
            if (column == null) return;

            var taskVm = new TaskViewModel();

            // Створюємо вікно БЕЗ властивості Owner
            var taskWindow = new Views.TaskWindow
            {
                DataContext = taskVm
            };

            taskWindow.ShowDialog();

            if (taskVm.IsSaved)
            {
                var newTaskDto = new CreateTaskDto
                {
                    Title = taskVm.Title,
                    Description = taskVm.Description,
                    ColumnId = column.Id,
                    Priority = "Medium"
                };

                var createdTask = await _apiService.CreateTaskAsync(newTaskDto);

                if (createdTask != null)
                {
                    await LoadBoardAsync();
                }
            }
        }

        [RelayCommand]
        private async Task DeleteTaskAsync(TaskDto task)
        {
            if (task == null) return;

            var result = MessageBox.Show($"Ви дійсно хочете видалити завдання «{task.Title}»?",
        "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if(result == MessageBoxResult.Yes)
            {
                var success = await _apiService.DeleteTaskAsync(task.Id);

                if(success)
                {
                    await LoadBoardAsync();
                } else
                {
                    MessageBox.Show("Помилка при видаленні завдання.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
        }
    }
}
