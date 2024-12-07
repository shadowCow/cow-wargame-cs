using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace game_client;

public class StatusPanel : INotifyPropertyChanged
{
    string _currentPlayerTurn = "Player 1";
    string _turnPhase = "Attacking";

    public string CurrentPlayerTurn
    {
        get => _currentPlayerTurn;
        set
        {
            if (_currentPlayerTurn != value)
            {
                _currentPlayerTurn = value;
                OnPropertyChanged(_currentPlayerTurn);
            }
        }
    }

    public string TurnPhase
    {
        get => _turnPhase;
        set
        {
            if (_turnPhase != value)
            {
                _turnPhase = value;
                OnPropertyChanged(_turnPhase);
            }
        }
    }

    public StatusPanel()
    {
        CurrentPlayerTurn = "Player 1";
        TurnPhase = "Attacking";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
