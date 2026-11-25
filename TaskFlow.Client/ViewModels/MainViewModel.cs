using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TaskFlow.Client.Services;
using TaskFlow.Shared.DTOs;

namespace TaskFlow.Client.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDropTarget
    {
        private readonly ApiService _apiService;
        private readonly DispatcherTimer _deadlineTimer;
        private readonly HashSet<int> _notifiedTaskIds = new();

        [ObservableProperty]
        private string _userNameDisplay = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ColumnDto> _columns = new();

        public MainViewModel()
        {
            _apiService = new ApiService();

            if (App.CurrentUser != null)
            {
                UserNameDisplay = $"Користувач: {App.CurrentUser.Username}";
            }

            _deadlineTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _deadlineTimer.Tick += (_, _) => CheckDeadlines();
            _deadlineTimer.Start();
        }

        private void CheckDeadlines()
        {
            if (Columns == null || Columns.Count == 0)
                return;

            var now = DateTime.Now;

            foreach (var col in Columns)
            {
                if (col.Tasks == null) continue;

                foreach (var task in col.Tasks)
                {
                    if (!task.Deadline.HasValue)
                        continue;

                    // Дедлайн не чистимо і не ховаємо — він залишається на картці
                    if (now >= task.Deadline.Value && _notifiedTaskIds.Add(task.Id))
                    {
                        SystemSounds.Exclamation.Play();
                    }
                }
            }
        }

        public async Task LoadBoardAsync()
        {
            if (App.CurrentUser == null) return;

            var projects = await _apiService.GetUserProjectsAsync(App.CurrentUser.Id);

            ProjectDto? currentProject = null;

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

            if (currentProject != null)
            {
                var board = await _apiService.GetProjectBoardAsync(currentProject.Id);

                Columns.Clear();
                foreach (var col in board)
                {
                    foreach (var task in col.Tasks)
                    {
                        // Якщо дедлайн перенесли в майбутнє — дозволяємо звуку спрацювати знову при новому настанні
                        if (task.Deadline.HasValue && DateTime.Now < task.Deadline.Value)
                        {
                            _notifiedTaskIds.Remove(task.Id);
                        }
                    }

                    Columns.Add(col);
                }

                CheckDeadlines();
            }
        }

        [RelayCommand]
        private async Task AddTaskAsync(ColumnDto column)
        {
            if (column == null) return;

            var taskVm = new TaskViewModel();

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
                    Priority = taskVm.Priority ?? "Medium",
                    Deadline = taskVm.Deadline
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

            var result = MessageBox.Show(
                $"Ви дійсно хочете видалити завдання «{task.Title}»?",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var success = await _apiService.DeleteTaskAsync(task.Id);

                if (success)
                {
                    _notifiedTaskIds.Remove(task.Id);
                    await LoadBoardAsync();
                }
                else
                {
                    MessageBox.Show("Помилка при видаленні завдання.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task EditTaskAsync(TaskDto task)
        {
            if (task == null) return;

            string initialHour = task.Deadline.HasValue ? task.Deadline.Value.ToString("HH") : "00";
            string initialMinute = task.Deadline.HasValue ? task.Deadline.Value.ToString("mm") : "00";
            DateTime? initialDate = task.Deadline.HasValue ? task.Deadline.Value.Date : null;

            var taskVm = new TaskViewModel
            {
                TaskId = task.Id,
                ApiService = _apiService,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority ?? "Medium",
                Deadline = task.Deadline,
                SelectedDate = initialDate,
                SelectedHour = initialHour,
                SelectedMinute = initialMinute,
                Comments = new ObservableCollection<CommentDto>(task.Comments ?? new())
            };

            var taskWindow = new Views.TaskWindow
            {
                DataContext = taskVm
            };

            taskWindow.ShowDialog();
            task.Comments = taskVm.Comments.ToList();

            if (taskVm.IsSaved)
            {
                var updateDto = new UpdateTaskDto
                {
                    Title = taskVm.Title,
                    Description = taskVm.Description,
                    ColumnId = task.ColumnId,
                    Order = task.Order,
                    Priority = taskVm.Priority,
                    Deadline = task.Deadline
                };

                await _apiService.UpdateTaskAsync(task.Id, updateDto);
                _notifiedTaskIds.Remove(task.Id);
                await LoadBoardAsync();
            }
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ColumnDto || dropInfo.Data is TaskDto)
            {
                dropInfo.Effects = DragDropEffects.Move;
            }

            if (dropInfo.Data is TaskDto && dropInfo.TargetCollection != null)
            {
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);
            }
        }

        public async void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ColumnDto draggedColumn && dropInfo.TargetCollection is ObservableCollection<ColumnDto> targetColumns)
            {
                int oldIndex = targetColumns.IndexOf(draggedColumn);
                int newIndex = dropInfo.InsertIndex;
                if (newIndex >= targetColumns.Count) newIndex = targetColumns.Count - 1;

                if (oldIndex != newIndex)
                {
                    targetColumns.Move(oldIndex, newIndex);
                    UpdateColumnsOrderAsync(targetColumns);
                }
            }

            if (dropInfo.Data is TaskDto task)
            {
                var sourceColumn = Columns.FirstOrDefault(c => c.Tasks == dropInfo.DragInfo.SourceCollection);
                var targetColumn = Columns.FirstOrDefault(c => c.Tasks == dropInfo.TargetCollection);

                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);

                if (sourceColumn != null && targetColumn != null)
                {
                    if (sourceColumn.Id != targetColumn.Id)
                    {
                        await UpdateTaskColumnAsync(task, targetColumn.Id);
                    }
                }
            }
        }

        private async void UpdateColumnsOrderAsync(ObservableCollection<ColumnDto> columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                columns[i].Order = i;
                var updateDto = new UpdateColumnDto
                {
                    Name = columns[i].Name,
                    Order = i
                };
                await _apiService.UpdateColumnAsync(columns[i].Id, updateDto);
            }
        }

        private async Task UpdateTaskColumnAsync(TaskDto task, int newColumnId)
        {
            var updateDto = new UpdateTaskDto
            {
                Title = task.Title,
                Description = task.Description,
                ColumnId = newColumnId,
                Order = task.Order,
                Priority = task.Priority ?? "Medium",
                Deadline = task.Deadline
            };

            var success = await _apiService.UpdateTaskAsync(task.Id, updateDto);
            if (success)
            {
                await LoadBoardAsync();
            }
        }

        [RelayCommand]
        private async Task AddColumnAsync()
        {
            if (App.CurrentUser == null) return;

            var projects = await _apiService.GetUserProjectsAsync(App.CurrentUser.Id);
            var currentProject = projects.FirstOrDefault();

            if (currentProject == null)
            {
                MessageBox.Show("Спочатку створіть або виберіть проєкт!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int nextOrder = Columns.Count > 0 ? Columns.Max(c => c.Order) + 1 : 0;

            var newColumnDto = new CreateColumnDto
            {
                Name = $"Колонка {Columns.Count + 1}",
                ProjectId = currentProject.Id,
                Order = nextOrder
            };

            var createColumn = await _apiService.CreateColumnAsync(newColumnDto);
            if (createColumn != null)
            {
                await LoadBoardAsync();
            }
            else
            {
                MessageBox.Show("Не вдалося створити колонку.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditColumnAsync(ColumnDto column)
        {
            if (column == null) return;

            string newName = Microsoft.VisualBasic.Interaction.InputBox(
                "Введіть нову назву колонки:",
                "Редагування колонки",
                column.Name
            );

            if (!string.IsNullOrWhiteSpace(newName) && newName != column.Name)
            {
                var updateDto = new UpdateColumnDto
                {
                    Name = newName,
                    Order = column.Order
                };

                var succes = await _apiService.UpdateColumnAsync(column.Id, updateDto);
                if (succes)
                {
                    await LoadBoardAsync();
                }
                else
                {
                    MessageBox.Show("Не вдалосся оновити назву колонки.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}