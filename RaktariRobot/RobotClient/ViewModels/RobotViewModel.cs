using RobotShared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RobotClient.ViewModels
{
    public class RobotViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5090/") };

        private double _x = 0;
        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(); }
        }

        private double _y = 0;
        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(); }
        }

        private int _battery = 100;
        public int Battery
        {
            get => _battery;
            set { _battery = value; OnPropertyChanged(); }
        }

        private string _state = "Ready";
        public string State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(); }
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand MoveForwardCommand { get; }
        public ICommand PickUpCommand { get; }
        public ICommand EmergencyStopCommand { get; }
        public ICommand RefreshStateCommand { get; }

        public RobotViewModel()
        {
            MoveForwardCommand = new Command(MoveForward);
            PickUpCommand = new Command(PickUpItem);
            EmergencyStopCommand = new Command(EmergencyStop);
            RefreshStateCommand = new Command(FetchInitialState);

            FetchInitialState();
        }

        private async void FetchInitialState()
        {
            try
            {
                var state = await _httpClient.GetFromJsonAsync<RobotDetailsDto>("api/robot/state");
                if (state != null)
                {
                    UpdateUI(state);
                    ErrorMessage = "";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fetching initial state failed: {ex.Message}";
            }
        }

        private async void MoveForward()
        {
            try
            {
                var response = await _httpClient.PostAsync("api/robot/move-forward", null);

                if (response.IsSuccessStatusCode)
                {
                    var updatedState = await response.Content.ReadFromJsonAsync<RobotDetailsDto>();

                    if (updatedState != null)
                    {
                        UpdateUI(updatedState);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Network communication error: {ex.Message}";
            }
        }

        private async void PickUpItem()
        {
            State = RobotState.MovingToShelf.ToString();
            Battery -= 5; // placeholder

            // Később itt is érdemes lesz hívni a szervert, pl:
            // var response = await _httpClient.PostAsync("api/robot/pick-up", null);
        }

        private async void EmergencyStop()
        {
            State = RobotState.Error.ToString();
            ErrorMessage = "A robot manuálisan leállítva.";

            try
            {
                var response = await _httpClient.PutAsync("api/robot/emergency-stop", null);
                if (response.IsSuccessStatusCode)
                {
                    var updatedState = await response.Content.ReadFromJsonAsync<RobotDetailsDto>();
                    if (updatedState != null)
                    {
                        UpdateUI(updatedState);
                        ErrorMessage = updatedState.State == RobotState.Error ? "Robot stopped by the server!" : "Robot is active again";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Cannot reach the server: {ex.Message}";
            }
        }

        private void UpdateUI(RobotDetailsDto updatedState)
        {
            X = updatedState.Position.X;
            Y = updatedState.Position.Y;
            Battery = updatedState.BatteryLevel;
            State = updatedState.State.ToString();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
