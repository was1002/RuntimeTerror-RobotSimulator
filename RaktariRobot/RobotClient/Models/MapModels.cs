using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics; 

namespace RuntimeTerror.Client.Models;

public class MapCell
{
    public Rect Bounds { get; set; }
    public Color BgColor { get; set; } = Colors.Transparent;
    public string Icon { get; set; } = string.Empty;
    public Color TextColor { get; set; } = Colors.Transparent;
}

public class RobotMarker : INotifyPropertyChanged
{
    public int RobotId { get; set; }

    private Rect _bounds;
    public Rect Bounds
    {
        get => _bounds;
        set { _bounds = value; OnPropertyChanged(); }
    }

    private string _displayName = string.Empty;
    public string DisplayName
    {
        get => _displayName;
        set { _displayName = value; OnPropertyChanged(); }
    }

    private Color _robotColor = Colors.Blue;
    public Color RobotColor
    {
        get => _robotColor;
        set { _robotColor = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
