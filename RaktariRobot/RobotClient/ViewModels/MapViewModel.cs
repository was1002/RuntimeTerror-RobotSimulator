using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using RobotShared;
using RuntimeTerror.Client.Models;
using System.Linq;

namespace RuntimeTerror.Client.ViewModels
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<MapCell> MapCells { get; } = new();
        public ObservableCollection<RobotMarker> MapRobots { get; } = new();
        public double CellSize { get; } = 40;

        public void GenerateMap(WarehouseDto warehouse)
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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}