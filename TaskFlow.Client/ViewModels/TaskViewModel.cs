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
