using RuntimeTerror.Client;
using Xunit;
using RuntimeTerror.Client.Models;
using RobotShared;

namespace RobotTest.Client
{
    public class MainViewModelTest
    {
        // --- IsSimulationRunning Tests ---
        [Fact]
        public void IsSimulationRunning_DefaultsToFalse_And_RaisesPropertyChangedOnSet()
        {
            var vm = new MainViewModel();
            var changed = new List<string>();
            vm.PropertyChanged += (s, e) => changed.Add(e.PropertyName ?? "");
            Assert.False(vm.IsSimulationRunning);
            vm.IsSimulationRunning = true;
            Assert.True(vm.IsSimulationRunning);
            Assert.Contains("IsSimulationRunning", changed);
        }

        [Fact]
        public void ToggleSimulationCommand_TogglesIsSimulationRunning()
        {
            var vm = new MainViewModel();
            var initial = vm.IsSimulationRunning;
            vm.ToggleSimulationCommand.Execute(null);
            Assert.Equal(!initial, vm.IsSimulationRunning);

            vm.IsSimulationRunning = false;
            vm.ToggleSimulationCommand.Execute(null);
            Assert.True(vm.IsSimulationRunning);

            vm.IsSimulationRunning = true;
            vm.ToggleSimulationCommand.Execute(null);
            Assert.False(vm.IsSimulationRunning);
        }

        [Fact]
        public void SimulationButtonText_ReflectsIsSimulationRunning()
        {
            var vm = new MainViewModel();
            Assert.Equal("Start", vm.SimulationButtonText);
            vm.IsSimulationRunning = true;
            Assert.Equal("Stop", vm.SimulationButtonText);
        }

        // --- IsSimpleView Tests ---
        [Fact]
        public void IsSimpleView_DefaultsToTrue_And_RaisesPropertyChangedOnSet()
        {
            var vm = new MainViewModel();
            var changed = new List<string>();
            vm.PropertyChanged += (s, e) => changed.Add(e.PropertyName ?? "");

            Assert.True(vm.IsSimpleView);

            vm.IsSimpleView = false;

            Assert.False(vm.IsSimpleView);
            Assert.Contains("IsSimpleView", changed);
        }

        [Fact]
        public void ToggleViewCommand_TogglesIsSimpleView()
        {
            var vm = new MainViewModel();
            var initial = vm.IsSimpleView;

            vm.ToggleViewCommand.Execute(null);

            Assert.Equal(!initial, vm.IsSimpleView);
        }
        // --- end ---

        [Fact]
        public void SelectedRobot_SetAndClear_UpdatesHasSelectedRobotAndRaisesPropertyChanged()
        {
            var vm = new MainViewModel();
            var changed = new List<string>();
            vm.PropertyChanged += (s, e) => changed.Add(e.PropertyName ?? "");

            var dto = new RobotDetailsDto { RobotId = 42, DisplayName = "R42" };
            var obs = new ObservableRobot(dto);

            vm.SelectedRobot = obs;
            Assert.True(vm.HasSelectedRobot);
            Assert.Contains("SelectedRobot", changed);
            Assert.Contains("HasSelectedRobot", changed);

            changed.Clear();
            vm.SelectedRobot = null;
            Assert.False(vm.HasSelectedRobot);
            Assert.Contains("SelectedRobot", changed);
            Assert.Contains("HasSelectedRobot", changed);
        }

        [Fact]
        public void AppMessage_SetAndClear_RaisesPropertyChanged()
        {
            var vm = new MainViewModel();
            var changed = new List<string>();
            vm.PropertyChanged += (s, e) => changed.Add(e.PropertyName ?? "");

            Assert.Empty(vm.AppMessage);

            vm.AppMessage = "Hello";
            Assert.Equal("Hello", vm.AppMessage);
            Assert.Contains("AppMessage", changed);
        }

        [Fact]
        public void Constructor_InitializesProperties()
        {
            var vm = new MainViewModel();

            Assert.NotNull(vm.ToggleSimulationCommand);
            Assert.NotNull(vm.ResetSimulationCommand);
            Assert.NotNull(vm.ToggleViewCommand);
            Assert.NotNull(vm.AddRobotCommand);
            Assert.NotNull(vm.RemoveRobotCommand);
            Assert.NotNull(vm.RenameRobotCommand);
            Assert.NotNull(vm.ResumeRobotCommand);
            Assert.NotNull(vm.PauseRobotCommand);
            Assert.NotNull(vm.MoveToChargerCommand);
            Assert.NotNull(vm.MoveToServiceCommand);
            Assert.NotNull(vm.SetLocationCommand);
            Assert.NotNull(vm.ClearWarningCommand);
            Assert.NotNull(vm.FixErrorCommand);
            Assert.NotNull(vm.SimulateFaultCommand);
        }

        [Fact]
        public async Task Constructor_LoadWarehouse_InitializesMap_WhenServerReturnsWarehouse()
        {
            var warehouse = new WarehouseDto
            {
                Width = 3,
                Height = 2,
                SpawnPosition = new PositionDto { X = 0, Y = 0 },
                DropoffPosition = new PositionDto { X = 1, Y = 0 },
                ChargerPosition = new PositionDto { X = 2, Y = 0 },
                ServicePosition = new PositionDto { X = 0, Y = 1 },
                Shelves = new List<ShelfDto> { new ShelfDto { ShelfId = "1", Position = new PositionDto { X = 1, Y = 1 } } }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(warehouse);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            using var listener = new System.Net.HttpListener();
            listener.Prefixes.Add("http://localhost:5090/");
            listener.Start();

            var responder = Task.Run(async () =>
            {
                try
                {
                    while (listener.IsListening)
                    {
                        var ctx = await listener.GetContextAsync();
                        if (ctx.Request.HttpMethod == "GET" && ctx.Request.Url.AbsolutePath == "/api/warehouse")
                        {
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "application/json";
                            ctx.Response.ContentLength64 = bytes.Length;
                            await ctx.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                            ctx.Response.OutputStream.Close();
                        }
                        else
                        {
                            ctx.Response.StatusCode = 404;
                            ctx.Response.Close();
                        }
                    }
                }
                catch (ObjectDisposedException) { /* listener stopped */ }
            });

            try
            {
                // Construct the VM (its constructor calls LoadWarehouse and will hit our listener)
                var vm = new RuntimeTerror.Client.MainViewModel();

                // Wait until MapCells have been populated or timeout
                var expected = warehouse.Width * warehouse.Height;
                var sw = System.Diagnostics.Stopwatch.StartNew();
                while (vm.MapCells.Count != expected && sw.ElapsedMilliseconds < 5000)
                {
                    await Task.Delay(50);
                }

                Assert.Equal(expected, vm.MapCells.Count);
            }
            finally
            {
                listener.Stop();
                await Task.WhenAny(responder, Task.Delay(100)); // let responder finish
            }
        }
    }
}