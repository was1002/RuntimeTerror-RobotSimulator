using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RobotClient
{
    public partial class StatisticsPage : ContentPage
    {
        public StatisticsPage(IEnumerable<RuntimeTerror.Client.Models.ObservableRobot> robots)
        {
            InitializeComponent();
            BindingContext = new StatisticsViewModel(robots);
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }

    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private readonly IEnumerable<RuntimeTerror.Client.Models.ObservableRobot> _robots;

        public int TotalRobots { get; private set; }
        public int WorkingRobots { get; private set; }
        public int ChargingRobots { get; private set; }
        public int ErrorRobots { get; private set; }

        public ICommand RefreshCommand { get; }

        public StatisticsViewModel(IEnumerable<RuntimeTerror.Client.Models.ObservableRobot> robots)
        {
            _robots = robots;
            RefreshCommand = new Command(CalculateStats);
            CalculateStats();
        }

        private void CalculateStats()
        {
            // LINQ komolyabb használata kategóriák szerint (LINQ GroupBy)
            var statsGroup = _robots.GroupBy(r => 
                r.State == RobotShared.RobotState.Error ? "Error" : 
                r.State == RobotShared.RobotState.Charging ? "Charging" : 
                r.State == RobotShared.RobotState.Idle ? "Idle" : "Working"
            ).ToList();

            TotalRobots = _robots.Count();

            ErrorRobots = statsGroup.FirstOrDefault(g => g.Key == "Error")?.Count() ?? 0;
            ChargingRobots = statsGroup.FirstOrDefault(g => g.Key == "Charging")?.Count() ?? 0;
            WorkingRobots = statsGroup.FirstOrDefault(g => g.Key == "Working")?.Count() ?? 0;

            OnPropertyChanged(nameof(TotalRobots));
            OnPropertyChanged(nameof(WorkingRobots));
            OnPropertyChanged(nameof(ChargingRobots));
            OnPropertyChanged(nameof(ErrorRobots));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}