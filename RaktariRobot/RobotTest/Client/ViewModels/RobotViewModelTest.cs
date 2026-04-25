using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobotClient.ViewModels;
using Xunit;

namespace RobotTest.Client.ViewModels
{
    public class RobotViewModelTest
    {
        [Fact]
        public void RobotViewModel_CreatesCommands()
        {
            var viewModel = new RobotViewModel();
            Assert.NotNull(viewModel.MoveForwardCommand);
            Assert.NotNull(viewModel.PickUpCommand);
            Assert.NotNull(viewModel.EmergencyStopCommand);
            Assert.NotNull(viewModel.RefreshStateCommand);
        }
    }
}
