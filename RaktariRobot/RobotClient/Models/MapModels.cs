using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics; 

namespace RuntimeTerror.Client.Models;

public class MapCell
{
    public Rect Bounds { get; set; }
    public Color BgColor { get; set; }
    public string Icon { get; set; }
    public Color TextColor { get; set; }
}

public class RobotMarker : INotifyPropertyChanged
{
    private Rect _bounds;
    public Rect Bounds
    {
        get => _bounds;
        set { _bounds = value; OnPropertyChanged(); }
    }

    private string _displayName;
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
