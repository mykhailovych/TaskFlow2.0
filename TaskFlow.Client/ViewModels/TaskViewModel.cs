using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using TaskFlow.Client.Services;
using TaskFlow.Shared.DTOs;

namespace TaskFlow.Client.ViewModels
{
    public partial class TaskViewModel : ObservableObject
    {
        public int TaskId { get; set; }
        public ApiService? ApiService { get; set; }

        [ObservableProperty]
        ObservableCollection<CommentDto> _comments = new();
        [ObservableProperty]
        private string _newCommentText = string.Empty;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _priority = "Medium";

        [ObservableProperty]
        private DateTime? _deadline;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public bool IsSaved { get; private set; }
        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                UpdateDeadlineDateTime(); // Оновлюємо загальний Deadline при зміні дати
            }
        }

        private string _selectedHour = "00";
        public string SelectedHour
        {
            get => _selectedHour;
            set
            {
                _selectedHour = value;
                OnPropertyChanged();
                UpdateDeadlineDateTime();
            }
        }

        private string _selectedMinute = "00";
        public string SelectedMinute
        {
            get => _selectedMinute;
            set
            {
                _selectedMinute = value;
                OnPropertyChanged();
                UpdateDeadlineDateTime();
            }
        }

        private void UpdateDeadlineDateTime()
        {
            if (!SelectedDate.HasValue)
            {
                Deadline = null;
                return;
            }

            // Безпечний парсинг годин (якщо рядок пустий або null, ставимо 0)
            int hour = 0;
            if (!string.IsNullOrWhiteSpace(SelectedHour))
            {
                int.TryParse(SelectedHour.Trim(), out hour);
            }

            // Безпечний парсинг хвилин
            int minute = 0;
            if (!string.IsNullOrWhiteSpace(SelectedMinute))
            {
                int.TryParse(SelectedMinute.Trim(), out minute);
            }

            // Жорсткий захист меж
            hour = Math.Clamp(hour, 0, 23);
            minute = Math.Clamp(minute, 0, 59);

            // Формуємо новий дедлайн
            Deadline = new DateTime(
                SelectedDate.Value.Year,
                SelectedDate.Value.Month,
                SelectedDate.Value.Day,
                hour,
                minute,
                0
            );
        }


        [RelayCommand]
        private void Save(Window window)
        {
            if(string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Назва завдання обов'язкова!";
                return;
            }

            IsSaved = true;
            window.Close();

        }

        [RelayCommand]
        private void Cancel(Window window)
        {
            IsSaved = false;
            window.Close();
        }

        [RelayCommand]
        private async Task AddCommentAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCommentText) || TaskId == 0 || ApiService == null) return;

            var dto = new CreateCommentDto { Text = NewCommentText };
            var comment = await ApiService.AddCommentAsync(TaskId, dto);

            if(comment != null)
            {
                Comments.Add(comment);
                NewCommentText = string.Empty;
            }
        }
    }
}
