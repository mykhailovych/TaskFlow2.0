using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using TaskFlow.Client.Services;
using TaskFlow.Shared.DTOs;
using System.Windows.Media;


namespace TaskFlow.Client.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly LoginHistoryService _loginHistoryService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        
        private string _statusColor = "#DC2626";

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ObservableCollection<SavedLoginItem> _savedLogins = new();

        [ObservableProperty]
        private SavedLoginItem? _selectedSavedLogin;

        [ObservableProperty]
        private string _loginHint = string.Empty;

        [ObservableProperty]
        private bool _hasSavedLogins;

        public LoginViewModel()
        {
            _apiService = new ApiService();
            _loginHistoryService = new LoginHistoryService();
            PropertyChanged += LoginViewModel_PropertyChanged;
            LoadSavedLogins();
        }

        private void LoginViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedSavedLogin) && SelectedSavedLogin != null)
            {
                Username = SelectedSavedLogin.Username;
                Password = SelectedSavedLogin.Password;
                LoginHint = $"Ви вже логінилися як {SelectedSavedLogin.Username} ({SelectedSavedLogin.LastLogin:dd.MM.yyyy HH:mm}). Пароль підставлено.";
            }
        }

        private void LoadSavedLogins()
        {
            var items = _loginHistoryService.GetSavedLogins();
            SavedLogins = new ObservableCollection<SavedLoginItem>(items);
            HasSavedLogins = SavedLogins.Count > 0;

            if (HasSavedLogins)
            {
                SelectedSavedLogin = SavedLogins[0];
            }
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

            _loginHistoryService.SaveLogin(Username.Trim(), Password);
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
                StatusColor = "#DC2626";
                ErrorMessage = "Користувач з таким ім'ям вже існує.";
                return;
            }
            StatusColor = "#16A34A";
            ErrorMessage = "Успішна реєстрація! Тепер натисніть «Увійти».";
        }
    }
}