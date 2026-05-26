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

        [Fact]
        public void ServerAddress_Setter_ShouldUpdateValue()
        {
            // Arrange
            var viewModel = new SettingsViewModel();
            var newAddress = "http://example.com:1234/";
            // Act
            viewModel.ServerAddress = newAddress;
            // Assert
            Assert.Equal(newAddress, viewModel.ServerAddress);
        }

        [Fact]
        public void SaveCommand_ShouldExecuteWithoutErrors()
        {
            // Arrange
            var viewModel = new SettingsViewModel();
            // Act & Assert
            var exception = Record.Exception(() => viewModel.SaveCommand.Execute(null));
            Assert.Null(exception);
        }

        [Fact]
        public void ServerAddress_Setter_ShouldRaisePropertyChanged()
        {
            // Arrange
            var viewModel = new SettingsViewModel();
            var ServerAddressChangedRaised = false;
            // subscribe to the PropertyChanged event
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.ServerAddress))
                {
                    ServerAddressChangedRaised = true;
                }
            };
            // Act
            viewModel.ServerAddress = "http://newaddress.com:5678/";
            // Assert
            Assert.True(ServerAddressChangedRaised);
        }
    }
}
