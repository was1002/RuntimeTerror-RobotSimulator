using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RuntimeTerror.Client
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private double _x = 0;
        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(); }
        }

        private double _y = 0;
        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(); }
        }

        private int _battery = 100;
        public int Battery
        {
            get => _battery;
            set { _battery = value; OnPropertyChanged(); }
        }

        private string _state = "Készenlét (Üres)";
        public string State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(); }
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        // --- PARANCSOK (ICommand) ---
        public ICommand MoveForwardCommand { get; }
        public ICommand PickUpCommand { get; }
        public ICommand EmergencyStopCommand { get; }

        public MainViewModel()
        {
            MoveForwardCommand = new RelayCommand(MoveForward);
            PickUpCommand = new RelayCommand(PickUpItem);
            EmergencyStopCommand = new RelayCommand(EmergencyStop);
        }

        // Késöbb ide kell a szerver felé az üzenet küldés is a függvényekbe
        private void MoveForward()
        {
            Y += 1.5; 
            State = "Mozgás előre...";
        }

        private void PickUpItem()
        {
            State = "Rakomány felvéve";
            Battery -= 5; // Szimuláljuk, hogy a felvétel energiába kerül
        }

        private void EmergencyStop()
        {
            State = "VÉSZMEGÁLLÁS!";
            ErrorMessage = "A robot manuálisan leállítva.";
        }

        // --- Alap MVVM boilerplate kód ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}