using Xunit;

using RobotClient.ViewModels;
using RuntimeTerror.RobotClient.Models;

namespace RobotTest.Client.ViewModels
{
    public class SettingsViewModelTest
    {
        [Fact]
        public void CreateSettingsViewModel_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var viewModel = new SettingsViewModel();
            // Assert
            Assert.Equal("http://localhost:5090/", viewModel.ServerAddress);
        }
    }
}
