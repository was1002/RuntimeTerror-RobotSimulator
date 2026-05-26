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

        private Color _appMessageColor = Colors.White;
        public Color AppMessageColor
        {
            get => _appMessageColor;
            set { _appMessageColor = value; OnPropertyChanged(); }
        }

        public int ManualTargetX { get; set; }
        public int ManualTargetY { get; set; }

        private string _inputNewName = "robot-1";
        public string InputNewName 
        {
            get => _inputNewName;
            set
            {
                _inputNewName = value;
                OnPropertyChanged();
            }
        }

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
        public ICommand RunSelfTestCommand { get; }
        public ICommand OpenStatisticsCommand { get; }

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
            
            RunSelfTestCommand = new Command(async () => await RunSelfTest());
            OpenStatisticsCommand = new Command(async () => await OpenStatisticsWindow());

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
                AppMessageColor = Colors.Red;
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

            foreach (var robot in newRobotsList)
            {
                Color robotColor = Color.FromRgb(144, 238, 144);
                if (robot.State == RobotState.Error) robotColor = Colors.Red;
                else if (robot.State == RobotState.Paused || robot.State == RobotState.Charging) robotColor = Colors.Yellow;
                else if (robot.State != RobotState.Idle) robotColor = Colors.Blue;

                var existingListIndex = RobotsList.ToList().FindIndex(r => r.RobotId == robot.RobotId);
                if (existingListIndex >= 0)
                {
                    var existingItem = RobotsList[existingListIndex];

                    bool isOldError = existingItem.DiagnosticLevel == DiagnosticLevel.Error || existingItem.State == RobotState.Error;
                    bool isOldWarning = !isOldError && (existingItem.DiagnosticLevel == DiagnosticLevel.Warning || existingItem.DiagnosticLevel == DiagnosticLevel.CriticalWarning);
                    int oldCategory = isOldError ? 2 : (isOldWarning ? 1 : 0);

                    bool isNewError = robot.DiagnosticLevel == DiagnosticLevel.Error || robot.State == RobotState.Error;
                    bool isNewWarning = !isNewError && (robot.DiagnosticLevel == DiagnosticLevel.Warning || robot.DiagnosticLevel == DiagnosticLevel.CriticalWarning);
                    int newCategory = isNewError ? 2 : (isNewWarning ? 1 : 0);

                    if (oldCategory != newCategory)
                    {
                        RobotsList[existingListIndex] = new ObservableRobot(robot);
                    }
                    else
                    {
                        existingItem.UpdateFromDto(robot);
                    }
                }
                else
                {
                    RobotsList.Add(new ObservableRobot(robot));
                }

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
            if (SelectedRobot == null) 
            {
                AppMessageColor = Colors.Red;
                AppMessage = "No robot selected.";
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
                    AppMessageColor = Color.FromRgb(144, 238, 144);
                    AppMessage = "Command executed successfully.";
                    await RefreshRobotsAsync();
                }
                else
                {
                    AppMessageColor = Colors.Red;
                    AppMessage = $"Command failed. Status code: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                AppMessageColor = Colors.Red;
                AppMessage = $"Command error: {ex.Message}";
            }
        }

        private async Task AddRobot()
        {
            try
            {
                int nextIndex = RobotsList.Count + 1;
                string dynamicName = $"robot-{nextIndex}";

                var req = new CreateRobotRequestDto { DisplayName = dynamicName };
                var response = await _httpClient.PostAsJsonAsync("api/robots", req);
                if (response.IsSuccessStatusCode)
                {
                    AppMessageColor = Color.FromRgb(144, 238, 144);
                    AppMessage = "Robot added.";
                    await RefreshRobotsAsync();

                    int futureIndex = RobotsList.Count + 1;
                    InputNewName = $"robot-{futureIndex}";
                }
            }
            catch (Exception ex) 
            { 
                AppMessageColor = Colors.Red;
                AppMessage = $"Add error: {ex.Message}";
            }
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
            catch (Exception ex) 
            {
                AppMessageColor = Colors.Red;
                AppMessage = $"Rename error: {ex.Message}"; 
            }
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
                    AppMessageColor = Color.FromRgb(144, 238, 144);
                    AppMessage = $"Location set to {req.X}, {req.Y}.";
                    await RefreshRobotsAsync();
                }
                else
                {
                    AppMessageColor = Colors.Red;
                    AppMessage = $"Location is out of bounds.";
                }
            }
            catch (Exception ex) 
            {
                AppMessageColor = Colors.Red;
                AppMessage = $"Location error: {ex.Message}";
            }
        }

        private async Task ResetSimulation()
        {
            try
            {
                var res = await _httpClient.PostAsync("api/simulation/reset", null);
                if (res.IsSuccessStatusCode)
                {
                    AppMessageColor = Color.FromRgb(144, 238, 144);
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
            catch (Exception ex)
            {
                AppMessageColor = Colors.Red;
                AppMessage = $"Reset error: {ex.Message}";
            }
        }

        private async Task RefreshRobotsAsync()
        {
            if(IsSimulationRunning) return; 
            try
            {
                var robots = await _httpClient.GetFromJsonAsync<List<RobotDetailsDto>>("api/robots");
                if (robots != null) UpdateRobotsData(robots);
            }
            catch { /* Ignore async refresh errors for brevity */ }
        }

        private async Task RunSelfTest()
        {
            if (SelectedRobot == null)
            {
                AppMessageColor = Colors.Red;
                AppMessage = "No robot selected for self-test.";
                return;
            }

            try
            {
                var response = await _httpClient.PostAsync($"api/robots/{SelectedRobot.RobotId}/self-test", null);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<SelfTestResultDto>();
                    if (result != null)
                    {
                        AppMessageColor = Color.FromRgb(144, 238, 144);
                        AppMessage = $"Self-Test ({result.DisplayName}): {result.Summary}";
                        if (Application.Current?.Windows.Count > 0 && Application.Current.Windows[0].Page != null)
                        {
                            await Application.Current.Windows[0].Page.DisplayAlert("Self-Test Result", result.Summary + (result.Success ? "" : "\n\nPlease check diagnostics array."), "OK");
                        }
                    }
                }
                else
                {
                    AppMessageColor = Colors.Red;
                    AppMessage = $"Self-test failed to execute. Status: {response.StatusCode}";
                }
                await RefreshRobotsAsync();
            }
            catch (Exception ex) 
            {
                AppMessageColor = Colors.Red;
                AppMessage = $"Self-test error: {ex.Message}";
            }
        }

        private async Task OpenStatisticsWindow()
        {
            var statsPage = new global::RobotClient.StatisticsPage(RobotsList);
            if (Application.Current?.Windows.Count > 0)
            {
                var mainPage = Application.Current.Windows[0].Page;
                if (mainPage != null)
                {
                    await mainPage.Navigation.PushModalAsync(statsPage);
                }
            }
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