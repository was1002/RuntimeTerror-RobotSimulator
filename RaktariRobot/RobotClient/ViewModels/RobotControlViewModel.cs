using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using RobotShared;
using RuntimeTerror.Client.Models;
using System;
using System.Threading.Tasks;

namespace RuntimeTerror.Client.ViewModels
{
    public class RobotControlViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private readonly Action<string, string> _messageCallback;
        private readonly Func<Task> _refreshCallback;

        public RobotControlViewModel(HttpClient httpClient, Action<string, string> messageCallback, Func<Task> refreshCallback)
        {
            _httpClient = httpClient;
            _messageCallback = messageCallback;
            _refreshCallback = refreshCallback;

            ResumeRobotCommand = new Command(async () => await ExecuteRobotCommand("POST", "resume"));
            PauseRobotCommand = new Command(async () => await ExecuteRobotCommand("POST", "pause"));
            MoveToChargerCommand = new Command(async () => await ExecuteRobotCommand("POST", "move-to-charger"));
            MoveToServiceCommand = new Command(async () => await ExecuteRobotCommand("POST", "move-to-service"));
            SetLocationCommand = new Command(async () => await SetManualLocation());

            ClearWarningCommand = new Command(async () => await ExecuteRobotCommand("POST", "clear-warning"));
            FixErrorCommand = new Command(async () => await ExecuteRobotCommand("POST", "fix-error"));
            SimulateFaultCommand = new Command(async () => await ExecuteRobotCommand("POST", "simulate-fault"));

            RunSelfTestCommand = new Command(async () => await RunSelfTest());
        }

        private ObservableRobot? _selectedRobot;
        public ObservableRobot? SelectedRobot
        {
            get => _selectedRobot;
            set { _selectedRobot = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedRobot)); }
        }

        public bool HasSelectedRobot => SelectedRobot != null;

        public int ManualTargetX { get; set; }
        public int ManualTargetY { get; set; }

        public ICommand ResumeRobotCommand { get; }
        public ICommand PauseRobotCommand { get; }
        public ICommand MoveToChargerCommand { get; }
        public ICommand MoveToServiceCommand { get; }
        public ICommand SetLocationCommand { get; }
        public ICommand ClearWarningCommand { get; }
        public ICommand FixErrorCommand { get; }
        public ICommand SimulateFaultCommand { get; }
        public ICommand RunSelfTestCommand { get; }

        private async Task ExecuteRobotCommand(string method, string endpointSuffix)
        {
            if (SelectedRobot == null) 
            {
                _messageCallback("No robot selected.", "Red");
                return;
            }

            try
            {
                string url = $"api/robots/{SelectedRobot.RobotId}/{endpointSuffix}";
                HttpResponseMessage response;

                if (method == "DELETE")
                {
                     url = $"api/robots/{SelectedRobot.RobotId}";
                     response = await _httpClient.DeleteAsync(url);
                }
                else
                {
                     response = await _httpClient.PostAsync(url, null);
                }

                if (response.IsSuccessStatusCode)
                {
                    _messageCallback("Command executed successfully.", "Green");
                    await _refreshCallback();
                }
                else
                {
                    _messageCallback($"Command failed. Status code: {response.StatusCode}", "Red");
                }
            }
            catch (Exception ex)
            {
                _messageCallback($"Command error: {ex.Message}", "Red");
            }
        }

        private async Task SetManualLocation()
        {
            if (SelectedRobot == null)
            {
                _messageCallback("No robot selected.", "Red");
                return;
            }
            try
            {
                var req = new MoveToLocationRequestDto { X = ManualTargetX, Y = ManualTargetY };
                var response = await _httpClient.PostAsJsonAsync($"api/robots/{SelectedRobot.RobotId}/move-to-location", req);
                if (response.IsSuccessStatusCode)
                {
                    _messageCallback("Target location set.", "Green");
                    await _refreshCallback();
                }
                else
                {
                     var msg = await response.Content.ReadAsStringAsync();
                    _messageCallback($"Failed to set location: {msg}", "Red");
                }
            }
            catch (Exception ex)
            {
                _messageCallback($"Location error: {ex.Message}", "Red");
            }
        }

        private async Task RunSelfTest()
        {
            if (SelectedRobot == null)
            {
                _messageCallback("No robot selected.", "Red");
                return;
            }
            try
            {
                var response = await _httpClient.PostAsync($"api/robots/{SelectedRobot.RobotId}/self-test", null);
                if (response.IsSuccessStatusCode)
                {
                     var result = await response.Content.ReadFromJsonAsync<SelfTestResultDto>();
                     string passStatus = result != null && result.Success ? "PASSED" : "FAILED";
                    _messageCallback($"Self-test {passStatus} for robot {SelectedRobot.RobotId}. Message: {result?.Summary}", result != null && result.Success ? "Green" : "Red");
                     await _refreshCallback();
                }
            }
            catch (Exception ex)
            {
                _messageCallback($"Self-test error: {ex.Message}", "Red");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
