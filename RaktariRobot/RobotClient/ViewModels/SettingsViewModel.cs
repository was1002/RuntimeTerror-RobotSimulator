using RuntimeTerror.RobotClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;

namespace RobotClient.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private ConnectionSettingsModel _settings = new();

        public string ServerAddress
        {
            get => _settings.ServerAddress;
            set { _settings.ServerAddress = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        public SettingsViewModel()
        {
            SaveCommand = new Command(SaveSettings);
        }

        private void SaveSettings()
        {
            Console.WriteLine($"Saved: {ServerAddress}");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
