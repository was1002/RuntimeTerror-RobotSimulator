using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Net.Http.Json;
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

        private string _state = "Készenlét (Üres)";
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
            MoveForwardCommand = new RelayCommand(MoveForward);
            PickUpCommand = new RelayCommand(PickUpItem);
            EmergencyStopCommand = new RelayCommand(EmergencyStop);
        }

        // Késöbb ide kell a szerver felé az üzenet küldés is a függvényekbe
        private async void MoveForward()
        {
            try
            {
                // HTTP POST kérés küldése, ami várja a frissített JSON állapotot
                var response = await _httpClient.PostAsync("api/robot/move-forward", null);

                if (response.IsSuccessStatusCode)
                {
                    // JSON deszerializáció a saját típusunkba
                    var updatedState = await response.Content.ReadFromJsonAsync<RobotStateDto>();

                    if (updatedState != null)
                    {
                        // UI frissítése az új adatokkal
                        X = updatedState.X;
                        Y = updatedState.Y;
                        Battery = updatedState.Battery;
                        State = updatedState.StateMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Hiba a hálózati kommunikációban: {ex.Message}";
            }
        }

        private void PickUpItem()
        {
            State = "Rakomány felvéve";
            Battery -= 5; // Szimuláljuk, hogy a felvétel energiába kerül
        }

        private async void EmergencyStop()
        {
            try
            {
                var response = await _httpClient.PutAsync("api/robot/emergency-stop", null);
                if (response.IsSuccessStatusCode)
                {
                    var updatedState = await response.Content.ReadFromJsonAsync<RobotStateDto>();
                    if (updatedState != null)
                    {
                        State = updatedState.StateMessage;
                        ErrorMessage = "A szerver leállította a robotot!";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Nem sikerült elérni a szervert!";
            }
        }

        // --- Alap MVVM boilerplate kód ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}