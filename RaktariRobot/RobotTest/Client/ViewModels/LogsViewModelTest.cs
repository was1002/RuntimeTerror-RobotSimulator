using RobotClient.ViewModels;
using RobotShared;
using Xunit;

namespace RobotTest.Client.ViewModels
{
    public class LogsViewModelTest
    {
        [Fact]
        public void CreateLogsViewModel_ShouldInitializeWithDefaultLog()
        {
            // Arrange & Act
            var viewModel = new LogsViewModel();
            // Assert
            Assert.Single(viewModel.Logs);
            Assert.NotNull(viewModel.ClearLogsCommand);
            var logEntry = viewModel.Logs[0];
            Assert.Equal("System started", logEntry.Message);
            Assert.Equal(DiagnosticLevel.Normal, logEntry.Level);
        }

        [Fact]
        public void AddLog_ShouldAddLogEntry()
        {
            // Arrange
            var viewModel = new LogsViewModel();
            string testMessage = "Test log message";
            DiagnosticLevel testLevel = DiagnosticLevel.Warning;
            // Act
            viewModel.AddLog(testMessage, testLevel);
            // Assert
            Assert.Equal(2, viewModel.Logs.Count());
            var logEntry = viewModel.Logs[1];
            Assert.Equal(testMessage, logEntry.Message);
            Assert.Equal(testLevel, logEntry.Level);
        }

        [Fact]
        public void ClearLogsCommand_ShouldClearAllLogs()
        {
            // Arrange
            var viewModel = new LogsViewModel();
            viewModel.AddLog("Another log", DiagnosticLevel.Error);
            Assert.Equal(2, viewModel.Logs.Count());
            // Act
            viewModel.ClearLogsCommand.Execute(null);
            // Assert
            Assert.Empty(viewModel.Logs);

        }

        [Fact]
        public void LogsCollection_ShouldNotifyChanges()
        {
            // Arrange
            var viewModel = new LogsViewModel();
            bool logsChanged = false;
            viewModel.Logs.CollectionChanged += (s, e) => logsChanged = true;
            // Act
            viewModel.AddLog("New log entry");
            // Assert
            Assert.True(logsChanged);
        }
    }
}
