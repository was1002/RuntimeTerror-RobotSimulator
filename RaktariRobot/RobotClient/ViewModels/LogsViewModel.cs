using Microsoft.Extensions.Logging.Abstractions;
using RobotShared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RuntimeTerror.RobotClient.Models;
using System.Runtime.CompilerServices;

namespace RobotClient.ViewModels
{
    public class LogsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<LogEntryModel> Logs { get; set; } = new();

        public ICommand ClearLogsCommand { get; }

        public LogsViewModel()
        {
            ClearLogsCommand = new Command(() => Logs.Clear());

            Logs.Add(new LogEntryModel { Message = "System started", Level =  DiagnosticLevel.Normal });
        }

        public void AddLog(string message, DiagnosticLevel level = DiagnosticLevel.Normal)
        {
            Logs.Add(new LogEntryModel { Message = message, Level = level });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
