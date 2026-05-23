using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using RobotShared;
using RuntimeTerror.Client.Models;
using System.Diagnostics;

namespace RuntimeTerror.Client
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5090/") };

        // --- Map Properties ---
        public ObservableCollection<MapCell> MapCells { get; } = new();
        public ObservableCollection<RobotMarker> MapRobots { get; } = new();
        public double CellSize { get; } = 40; 

        // --- Robot List Properties ---
        public ObservableCollection<ObservableRobot> RobotsList { get; } = new();

        private ObservableRobot? _selectedRobot;
        public ObservableRobot? SelectedRobot
        {
            get => _selectedRobot;
            set { _selectedRobot = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedRobot)); }
        }

        public bool HasSelectedRobot => SelectedRobot != null;

        // --- Simulation State ---
        private bool _isSimulationRunning;
        public bool IsSimulationRunning
        {
            get => _isSimulationRunning;
            set 
            { 
                _isSimulationRunning = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(SimulationButtonText));
            }
        }
        
        public string SimulationButtonText => IsSimulationRunning ? "Stop" : "Start";

        // --- View State ---
        private bool _isSimpleView = true;
        public bool IsSimpleView
        {
            get => _isSimpleView;
            set
            {
                if (_isSimpleView != value)
                {
                    _isSimpleView = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsSimpleView));
                }
            }
        }

        private string _appMessage = "";
        public string AppMessage
        {
            get => _appMessage;
            set { _appMessage = value; OnPropertyChanged(); }
        }
        
        public int ManualTargetX { get; set; }
        public int ManualTargetY { get; set; }

        public string InputNewName { get; set; } = "ROBOT-NEW";

        private CancellationTokenSource? _tickCancellationTokenSource;

        // --- Commands ---
        public ICommand ToggleSimulationCommand { get; }
        public ICommand ResetSimulationCommand { get; }
        public ICommand ToggleViewCommand { get; }
        public ICommand AddRobotCommand { get; }
        public ICommand RemoveRobotCommand { get; }
        public ICommand RenameRobotCommand { get; }
        
        public ICommand ResumeRobotCommand { get; }
        public ICommand PauseRobotCommand { get; }
        public ICommand MoveToChargerCommand { get; }
        public ICommand MoveToServiceCommand { get; }
        public ICommand SetLocationCommand { get; }
        
        public ICommand ClearWarningCommand { get; }
        public ICommand FixErrorCommand { get; }
        public ICommand SimulateFaultCommand { get; }

        public MainViewModel()
        {
            ToggleSimulationCommand = new Command(ToggleSimulation);
            ResetSimulationCommand = new Command(async () => await ResetSimulation());
            ToggleViewCommand = new Command(ToggleView);

            AddRobotCommand = new Command(async () => await AddRobot());
            RemoveRobotCommand = new Command(async () => await ExecuteRobotCommand("DELETE", ""));
            RenameRobotCommand = new Command(async () => await RenameRobot());

            ResumeRobotCommand = new Command(async () => await ExecuteRobotCommand("POST", "resume"));
            PauseRobotCommand = new Command(async () => await ExecuteRobotCommand("POST", "pause"));
            MoveToChargerCommand = new Command(async () => await ExecuteRobotCommand("POST", "move-to-charger"));
            MoveToServiceCommand = new Command(async () => await ExecuteRobotCommand("POST", "move-to-service"));
            SetLocationCommand = new Command(async () => await SetManualLocation());
            
            ClearWarningCommand = new Command(async () => await ExecuteRobotCommand("POST", "clear-warning"));
            FixErrorCommand = new Command(async () => await ExecuteRobotCommand("POST", "fix-error"));
            SimulateFaultCommand = new Command(async () => await ExecuteRobotCommand("POST", "simulate-fault"));

            LoadWarehouse();
        }

        private async void LoadWarehouse()
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
                AppMessage = $"Warehouse load error: {ex.Message}";
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

                    if (warehouse.SpawnPosition.X == x && warehouse.SpawnPosition.Y == y) { icon = "R"; bgColor = Colors.LightBlue; }
                    else if (warehouse.DropoffPosition.X == x && warehouse.DropoffPosition.Y == y) { icon = "D"; bgColor = Colors.Orange; }
                    else if (warehouse.ChargerPosition.X == x && warehouse.ChargerPosition.Y == y) { icon = "C"; bgColor = Colors.Yellow; }
                    else if (warehouse.ServicePosition.X == x && warehouse.ServicePosition.Y == y) { icon = "W"; bgColor = Colors.Magenta; }
                    else if (warehouse.Shelves.Any(s => s.Position.X == x && s.Position.Y == y)) { icon = "S"; bgColor = Colors.SaddleBrown; }

                    MapCells.Add(new MapCell
                    {
                        Icon = icon,
                        BgColor = bgColor,
                        TextColor = Colors.Black,
                        Bounds = new Rect(x * CellSize, y * CellSize, CellSize - 2, CellSize - 2)
                    });
                }
            }
        }

        private void ToggleSimulation()
        {
            if (IsSimulationRunning)
            {
                IsSimulationRunning = false;
                _tickCancellationTokenSource?.Cancel();
            }
            else
            {
                IsSimulationRunning = true;
                _tickCancellationTokenSource = new CancellationTokenSource();
                _ = Task.Run(() => SimulationTickLoopAsync(_tickCancellationTokenSource.Token));
            }
        }

        private async Task SimulationTickLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var response = await _httpClient.PostAsync("api/simulation/tick", null, token);
                    if (response.IsSuccessStatusCode)
                    {
                        var robots = await response.Content.ReadFromJsonAsync<List<RobotDetailsDto>>(cancellationToken: token);
                        if (robots != null)
                        {
                            MainThread.BeginInvokeOnMainThread(() => UpdateRobotsData(robots));
                        }
                    }
                }
                catch (TaskCanceledException) { /* Ignored */ }
                catch (Exception ex)
                {
                    MainThread.BeginInvokeOnMainThread(() => AppMessage = $"Tick error: {ex.Message}");
                }

                await Task.Delay(1000, token);
            }
        }

        private void UpdateRobotsData(List<RobotDetailsDto> newRobotsList)
        {
            int? currentSelectedId = SelectedRobot?.RobotId;

            var newIds = newRobotsList.Select(r => r.RobotId).ToHashSet();

            for (int i = RobotsList.Count - 1; i >= 0; i--)
            {
                if (!newIds.Contains(RobotsList[i].RobotId))
                    RobotsList.RemoveAt(i);
            }

            for (int i = MapRobots.Count - 1; i >= 0; i--)
            {
                if (!newIds.Contains(MapRobots[i].RobotId))
                    MapRobots.RemoveAt(i);
            }

            // Hozzáadás vagy meglévõk tulajdonságainak frissítése
            foreach (var robot in newRobotsList)
            {
                Color robotColor = Colors.Green;
                if (robot.State == RobotState.Error) robotColor = Colors.Red;
                else if (robot.State == RobotState.Paused || robot.State == RobotState.Charging) robotColor = Colors.Yellow;
                else if (robot.State != RobotState.Idle) robotColor = Colors.Blue;

                // 1. RobotsList (ide kötjük a UI listát)
                var existingListIndex = RobotsList.ToList().FindIndex(r => r.RobotId == robot.RobotId);
                if (existingListIndex >= 0)
                {
                    var existingItem = RobotsList[existingListIndex];

                    // Kiszámoljuk a sablon kategóriáját (0=Normál, 1=Figyelmeztetés, 2=Hiba)
                    bool isOldError = existingItem.DiagnosticLevel == DiagnosticLevel.Error || existingItem.State == RobotState.Error;
                    bool isOldWarning = !isOldError && (existingItem.DiagnosticLevel == DiagnosticLevel.Warning || existingItem.DiagnosticLevel == DiagnosticLevel.CriticalWarning);
                    int oldCategory = isOldError ? 2 : (isOldWarning ? 1 : 0);

                    bool isNewError = robot.DiagnosticLevel == DiagnosticLevel.Error || robot.State == RobotState.Error;
                    bool isNewWarning = !isNewError && (robot.DiagnosticLevel == DiagnosticLevel.Warning || robot.DiagnosticLevel == DiagnosticLevel.CriticalWarning);
                    int newCategory = isNewError ? 2 : (isNewWarning ? 1 : 0);

                    if (oldCategory != newCategory)
                    {
                        // Ha megváltozott a hibaállapot, kicseréljük az objektumot, 
                        // hogy a DataTemplateSelector újra kiértékelje és betöltse az új UI sablont.
                        RobotsList[existingListIndex] = new ObservableRobot(robot);
                    }
                    else
                    {
                        // Ha csak a pozíció vagy az aksi módosult, frissítjük az értékeket villogás nélkül.
                        existingItem.UpdateFromDto(robot);
                    }
                }
                else
                {
                    RobotsList.Add(new ObservableRobot(robot));
                }

                // 2. MapRobots (térkép markerek)
                var existingMarker = MapRobots.FirstOrDefault(m => m.RobotId == robot.RobotId);
                if (existingMarker != null)
                {
                    existingMarker.DisplayName = robot.DisplayName;
                    existingMarker.Bounds = new Rect(robot.Position.X * CellSize + 5, robot.Position.Y * CellSize + 5, CellSize - 12, CellSize - 12);
                    existingMarker.RobotColor = robotColor;
                }
                else
                {
                    MapRobots.Add(new RobotMarker
                    {
                        RobotId = robot.RobotId,
                        DisplayName = robot.DisplayName,
                        Bounds = new Rect(robot.Position.X * CellSize + 5, robot.Position.Y * CellSize + 5, CellSize - 12, CellSize - 12),
                        RobotColor = robotColor
                    });
                }
            }

            if (currentSelectedId.HasValue && SelectedRobot == null)
            {
                SelectedRobot = RobotsList.FirstOrDefault(r => r.RobotId == currentSelectedId.Value);
            }
        }

        private async Task ExecuteRobotCommand(string method, string endpointSuffix)
        {
            if (SelectedRobot == null) { AppMessage = "No robot selected."; return; }

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
                    AppMessage = "Command executed successfully.";
                    await RefreshRobotsAsync();
                }
                else
                {
                    AppMessage = $"Command failed. Status code: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                AppMessage = $"Command error: {ex.Message}";
            }
        }

        private async Task AddRobot()
        {
            try
            {
                var req = new CreateRobotRequestDto { DisplayName = InputNewName };
                var response = await _httpClient.PostAsJsonAsync("api/robots", req);
                if (response.IsSuccessStatusCode)
                {
                    AppMessage = "Robot added.";
                    await RefreshRobotsAsync();
                }
            }
            catch (Exception ex) { AppMessage = $"Add error: {ex.Message}"; }
        }

        private async Task RenameRobot()
        {
            if (SelectedRobot == null) return;
            try
            {
                var req = new RenameRobotRequestDto { NewDisplayName = InputNewName };
                var response = await _httpClient.PostAsJsonAsync($"api/robots/{SelectedRobot.RobotId}/rename", req);
                if (response.IsSuccessStatusCode)
                {
                    AppMessage = "Robot renamed.";
                    await RefreshRobotsAsync();
                }
            }
            catch (Exception ex) { AppMessage = $"Rename error: {ex.Message}"; }
        }

        private async Task SetManualLocation()
        {
            if (SelectedRobot == null) return;
            try
            {
                var req = new MoveToLocationRequestDto { X = ManualTargetX, Y = ManualTargetY };
                var response = await _httpClient.PostAsJsonAsync($"api/robots/{SelectedRobot.RobotId}/move-to-location", req);
                if (response.IsSuccessStatusCode)
                {
                    AppMessage = $"Location set to {req.X}, {req.Y}.";
                    await RefreshRobotsAsync();
                }
            }
            catch (Exception ex) { AppMessage = $"Location error: {ex.Message}"; }
        }

        private async Task ResetSimulation()
        {
            try
            {
                var res = await _httpClient.PostAsync("api/simulation/reset", null);
                if (res.IsSuccessStatusCode)
                {
                    AppMessage = "Simulation reset.";
                    var data = await res.Content.ReadFromJsonAsync<SimulationResetResponseDto>();
                    if (data != null)
                    {
                        GenerateMap(data.Warehouse);
                        UpdateRobotsData(data.Robots);
                        SelectedRobot = null;
                    }
                }
            }
            catch (Exception ex) { AppMessage = $"Reset error: {ex.Message}"; }
        }

        private async Task RefreshRobotsAsync()
        {
            if(IsSimulationRunning) return; // If running, tick handles it
            try
            {
                var robots = await _httpClient.GetFromJsonAsync<List<RobotDetailsDto>>("api/robots");
                if (robots != null) UpdateRobotsData(robots);
            }
            catch { /* Ignore async refresh errors for brevity */ }
        }

        private void ToggleView()
        {
            IsSimpleView = !IsSimpleView;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}