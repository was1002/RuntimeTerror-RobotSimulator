using Xunit;
using RuntimeTerror.Client.ViewModels;
using RuntimeTerror.Client.Models;
using RobotShared;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RobotTest.Client.ViewModels
{
    public class RobotControlViewModelTest
    {
        [Fact]
        public void Constructor_InitializesCommands()
        {
            var vm = new RobotControlViewModel(new HttpClient(), (m, c) => { }, () => Task.CompletedTask);

            Assert.NotNull(vm.ResumeRobotCommand);
            Assert.NotNull(vm.PauseRobotCommand);
            Assert.NotNull(vm.MoveToChargerCommand);
            Assert.NotNull(vm.MoveToServiceCommand);
            Assert.NotNull(vm.SetLocationCommand);
            Assert.NotNull(vm.ClearWarningCommand);
            Assert.NotNull(vm.FixErrorCommand);
            Assert.NotNull(vm.SimulateFaultCommand);
            Assert.NotNull(vm.RunSelfTestCommand);
        }

        [Fact]
        public void SelectedRobot_PropertyChanged_UpdatesHasSelectedRobot()
        {
            var vm = new RobotControlViewModel(new HttpClient(), (m, c) => { }, () => Task.CompletedTask);
            bool hasSelectedRobotChanged = false;

            vm.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(vm.HasSelectedRobot))
                    hasSelectedRobotChanged = true;
            };

            Assert.False(vm.HasSelectedRobot);

            vm.SelectedRobot = new ObservableRobot(new RobotDetailsDto { RobotId = 1 });

            Assert.True(vm.HasSelectedRobot);
            Assert.True(hasSelectedRobotChanged);
        }

        [Fact]
        public void AllCommands_WithNullSelectedRobot_CallsMessageCallback()
        {
            // Arrange
            string lastMessage = "";
            string lastColor = "";
            var vm = new RobotControlViewModel(new HttpClient(), (m, c) => { lastMessage = m; lastColor = c; }, () => Task.CompletedTask);

            vm.SelectedRobot = null; // No robot selected

            // Act & Assert
            var commands = new List<System.Windows.Input.ICommand> 
            {
                vm.ResumeRobotCommand,
                vm.PauseRobotCommand,
                vm.MoveToChargerCommand,
                vm.MoveToServiceCommand,
                vm.SetLocationCommand,
                vm.ClearWarningCommand,
                vm.FixErrorCommand,
                vm.SimulateFaultCommand,
                vm.RunSelfTestCommand
            };

            foreach (var command in commands)
            {
                lastMessage = "";
                lastColor = "";
                command.Execute(null);

                Assert.Equal("No robot selected.", lastMessage);
                Assert.Equal("Red", lastColor);
            }
        }
    }
}