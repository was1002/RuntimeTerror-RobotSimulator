using System.ComponentModel;
using System.Runtime.CompilerServices;
using RobotShared;

namespace RuntimeTerror.Client.Models;

public class ObservableRobot : INotifyPropertyChanged
{
    private RobotDetailsDto _dto;

    public ObservableRobot(RobotDetailsDto dto)
    {
        _dto = dto;
    }

    public int RobotId => _dto.RobotId;

    public string DisplayName 
    { 
        get => _dto.DisplayName; 
        set { if (_dto.DisplayName != value) { _dto.DisplayName = value; OnPropertyChanged(); } }
    }

    public RobotState State
    {
        get => _dto.State;
        set { if (_dto.State != value) { _dto.State = value; OnPropertyChanged(); } }
    }

    public int BatteryLevel
    {
        get => _dto.BatteryLevel;
        set { if (_dto.BatteryLevel != value) { _dto.BatteryLevel = value; OnPropertyChanged(); } }
    }

    public DiagnosticLevel DiagnosticLevel
    {
        get => _dto.DiagnosticLevel;
        set { if (_dto.DiagnosticLevel != value) { _dto.DiagnosticLevel = value; OnPropertyChanged(); } }
    }

    public string? LastErrorMessage
    {
        get => _dto.LastErrorMessage;
        set { if (_dto.LastErrorMessage != value) { _dto.LastErrorMessage = value; OnPropertyChanged(); } }
    }

    public void UpdateFromDto(RobotDetailsDto newDto)
    {
        // Don't update ID, it's immutable
        DisplayName = newDto.DisplayName;
        State = newDto.State;
        BatteryLevel = newDto.BatteryLevel;
        DiagnosticLevel = newDto.DiagnosticLevel;
        LastErrorMessage = newDto.LastErrorMessage;

        // internal DTO reference updated for unmapped properties:
        _dto = newDto;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}