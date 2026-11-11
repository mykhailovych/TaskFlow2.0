using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using TaskFlow.Client.Services;
using TaskFlow.Shared.DTOs;

namespace TaskFlow.Client.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        public LoginViewModel()
        {
            _apiService = new ApiService();
        }

        [RelayCommand]
        private async Task LoginAsync(Window window)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введіть ім'я користувача та пароль.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            var user = await _apiService.LoginAsync(new LoginDto { Username = Username, Password = Password });

            IsLoading = false;

            if (user == null)
            {
                ErrorMessage = "Невірний логін або пароль.";
                return;
            }

            App.CurrentUser = user;

            var mainWindow = new MainWindow();
            mainWindow.Show();
            window?.Close();
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Вкажіть ім'я користувача та пароль.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            var user = await _apiService.RegisterAsync(new RegisterDto { Username = Username, Password = Password });

            IsLoading = false;

            if (user == null)
            {
                ErrorMessage = "Користувач з таким ім'ям вже існує.";
                return;
            }

            ErrorMessage = "Успішна реєстрація! Тепер натисніть «Увійти».";
        }
    }
}