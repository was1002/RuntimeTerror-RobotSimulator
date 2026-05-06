using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using RobotShared;
using RuntimeTerror.Client.Models; // Models for MapCell and RobotMarker

namespace RuntimeTerror.Client
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5090/") };

        // Térképhez
        public ObservableCollection<MapCell> MapCells { get; } = new();
        public ObservableCollection<RobotMarker> Robots { get; } = new();
        public double CellSize { get; } = 40; // Pixel méret egy cellának

        // Térkép betöltése
        public ICommand LoadMapCommand { get; }

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

        // --- Commands ---
        public ICommand MoveForwardCommand { get; }
        public ICommand PickUpCommand { get; }
        public ICommand EmergencyStopCommand { get; }

        public MainViewModel()
        {
            LoadMapCommand = new Command(LoadMap);
            MoveForwardCommand = new Command(MoveForward);
            PickUpCommand = new Command(PickUpItem);
            EmergencyStopCommand = new Command(EmergencyStop);
            
            // Automatikus térképbetöltés
            LoadMap();
        }

        private async void LoadMap()
        {
            try
            {
                var warehouse = await _httpClient.GetFromJsonAsync<WarehouseDto>("api/warehouse");
                if (warehouse != null)
                {
                    GenerateMap(warehouse);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Térkép betöltési hiba: {ex.Message}";
            }
        }

        private void GenerateMap(WarehouseDto warehouse)
        {
            MapCells.Clear();

            for (int y = 0; y < warehouse.Height; y++)
            {
                for (int x = 0; x < warehouse.Width; x++)
                {
                    string icon = "";
                    Color bgColor = Colors.LightGray;

                    // Ellenõrizzük, mi van ezen a pozíción
                    if (warehouse.SpawnPosition.X == x && warehouse.SpawnPosition.Y == y) { icon = "R"; bgColor = Colors.LightBlue; }
                    else if (warehouse.DropoffPosition.X == x && warehouse.DropoffPosition.Y == y) { icon = "D"; bgColor = Colors.Orange; }
                    else if (warehouse.ChargerPosition.X == x && warehouse.ChargerPosition.Y == y) { icon = "C"; bgColor = Colors.Yellow; }
                    else if (warehouse.ServicePosition.X == x && warehouse.ServicePosition.Y == y) { icon = "W"; bgColor = Colors.Magenta; }
                    else if (warehouse.Shelves.Any(s => s.Position.X == x && s.Position.Y == y))
                    {
                        icon = "S";
                        bgColor = Colors.SaddleBrown;
                    }

                    MapCells.Add(new MapCell
                    {
                        Icon = icon,
                        BgColor = bgColor,
                        TextColor = Colors.Black,
                        Bounds = new Rect(x * CellSize, y * CellSize, CellSize - 2, CellSize - 2) // pici margó
                    });
                }
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
                        X = updatedState.Position.X;
                        Y = updatedState.Position.Y;
                        Battery = updatedState.BatteryLevel;
                        State = updatedState.State.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error in the network communication: {ex.Message}";
            }
        }

        private async void PickUpItem()
        {
            State = RobotState.MovingToShelf.ToString();
            Battery -= 5; // Helyi szimuláció

            // Késõbb itt is érdemes lesz hívni a szervert, pl:
            // var response = await _httpClient.PostAsync("api/robot/pick-up", null);
        }

        private async void EmergencyStop()
        {
            State = RobotState.Error.ToString();
            ErrorMessage = "A robot manuálisan leállítva.";
            
            try
            {
                var response = await _httpClient.PostAsync("api/robot/emergency-stop", null); // PUT helyett POST, az endpointtól függ
                if (response.IsSuccessStatusCode)
                {
                    var updatedState = await response.Content.ReadFromJsonAsync<RobotDetailsDto>();
                    if (updatedState != null)
                    {
                        State = updatedState.State.ToString();
                        ErrorMessage = updatedState.State == RobotState.Error ? "Robot stopped by the server!" : "Robot is active again.";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Cannot reach the server: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}