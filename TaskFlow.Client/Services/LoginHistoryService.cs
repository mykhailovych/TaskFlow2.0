using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TaskFlow.Client.Services
{
    public class SavedLoginItem
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime LastLogin { get; set; }
    }

    public class LoginHistoryService
    {
        private readonly string _filePath;

        public LoginHistoryService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "TaskFlow");
            Directory.CreateDirectory(folder);
            _filePath = Path.Combine(folder, "saved_logins.json");
        }

        public List<SavedLoginItem> GetSavedLogins()
        {
            if (!File.Exists(_filePath))
                return new List<SavedLoginItem>();

            try
            {
                var json = File.ReadAllText(_filePath);
                var items = JsonSerializer.Deserialize<List<SavedLoginItem>>(json) ?? new List<SavedLoginItem>();
                return items.OrderByDescending(x => x.LastLogin).ToList();
            }
            catch
            {
                return new List<SavedLoginItem>();
            }
        }

        public void SaveLogin(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return;

            var items = GetSavedLogins();
            var existing = items.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.Password = password;
                existing.LastLogin = DateTime.Now;
            }
            else
            {
                items.Add(new SavedLoginItem
                {
                    Username = username,
                    Password = password,
                    LastLogin = DateTime.Now
                });
            }

            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}