using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace TaskFlow.Client.ViewModels
{
    public partial class TaskViewModel : ObservableObject
    {
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
    }
}
