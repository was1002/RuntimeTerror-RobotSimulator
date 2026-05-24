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

        // --- MainViewModel Constructor Tests ---
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
    }
}