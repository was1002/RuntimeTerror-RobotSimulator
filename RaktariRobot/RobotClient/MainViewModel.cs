using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RobotShared;

namespace RuntimeTerror.Client
{
    public class MainViewModel : INotifyPropertyChanged
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

        // --- PARANCSOK (ICommand) ---
        public ICommand MoveForwardCommand { get; }
        public ICommand PickUpCommand { get; }
        public ICommand EmergencyStopCommand { get; }

        public MainViewModel()
        {
            MoveForwardCommand = new Command(MoveForward);
            PickUpCommand = new Command(PickUpItem);
            EmergencyStopCommand = new Command(EmergencyStop);
        }

        private async void MoveForward()
        {
            try
            {
                // Feltételezve, hogy az endpoint a teljes aktuális részletet (RobotDetailsDto) visszaadja
                var response = await _httpClient.PostAsync("api/robot/move-forward", null);

                if (response.IsSuccessStatusCode)
                {
                    var updatedState = await response.Content.ReadFromJsonAsync<RobotDetailsDto>();

                    if (updatedState != null)
                    {
                        // UI frissítése az új adatokkal
                        X = updatedState.Position.X;
                        Y = updatedState.Position.Y;
                        Battery = updatedState.BatteryLevel;
                        State = updatedState.State.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Hiba a hálózati kommunikációban: {ex.Message}";
            }
        }

        private async void PickUpItem()
        {
            State = RobotState.Loading.ToString();
            Battery -= 5; // Helyi szimuláció

            // Késõbb itt is érdemes lesz hívni a szervert, pl:
            // var response = await _httpClient.PostAsync("api/robot/pick-up", null);
        }

        private async void EmergencyStop()
        {
            State = RobotState.EmergencyStop.ToString();
            ErrorMessage = "A robot manuálisan leállítva.";
            
            try
            {
                var response = await _httpClient.PostAsync("api/robot/emergency-stop", null); // PUT helyett POST, az endpointtól függ
                if (response.IsSuccessStatusCode)
                {
                    var updatedState = await response.Content.ReadFromJsonAsync<RobotStateDto>();
                    if (updatedState != null)
                    {
                        State = updatedState.State.ToString();
                        ErrorMessage = updatedState.EmergencyStopActive ? "A szerver leállította a robotot!" : "A robot újra aktív.";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Nem sikerült elérni a szervert: {ex.Message}";
            }
        }

        // --- Alap MVVM boilerplate kód ---
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}