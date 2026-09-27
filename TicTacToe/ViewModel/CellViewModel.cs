using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using TicTacToe.Model;

namespace TicTacToe.ViewModel
{
    public sealed class CellViewModel : INotifyPropertyChanged
    {
        private readonly GameViewModel _parent;

        public CellViewModel(GameViewModel parent, int row, int column)
        {
            _parent = parent;
            Row = row;
            Column = column;
            MoveCommand = new RelayCommand(
                _ => _parent.HandleCellClick(this),
                _ => IsEnabled);
        }

        public int Row { get; }

        public int Column { get; }

        public ICommand MoveCommand { get; }

        public string Symbol => _parent.GetSymbol(Row, Column) switch
        {
            CellState.X => "X",
            CellState.O => "O",
            _ => string.Empty
        };

        public bool IsEnabled => _parent.CanClickCell(Row, Column);

        public void Refresh()
        {
            OnPropertyChanged(nameof(Symbol));
            OnPropertyChanged(nameof(IsEnabled));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
