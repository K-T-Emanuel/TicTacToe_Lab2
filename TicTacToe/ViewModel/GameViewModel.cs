using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TicTacToe.Model;

namespace TicTacToe.ViewModel;

public sealed class GameViewModel : INotifyPropertyChanged
{
    private const string NewGameHint = "Натисніть «Нова гра».";
    private const string HumanTurnText = "Ваш хід: X. Оберіть клітинку.";
    private const string ComputerTurnText = "Хід комп'ютера…";

    private readonly TicTacToeGame _game = new();
    private DifficultyLevel _difficulty = DifficultyLevel.Blocking;
    private string _statusMessage = string.Empty;

    public GameViewModel()
    {
        Cells = new ObservableCollection<CellViewModel>();
        for (int row = 0; row < TicTacToeGame.Size; row++)
            for (int column = 0; column < TicTacToeGame.Size; column++)
                Cells.Add(new CellViewModel(this, row, column));

        DifficultyOptions =
        [
            new DifficultyOption(DifficultyLevel.Random, "Рівень 1: випадковий хід"),
            new DifficultyOption(DifficultyLevel.Blocking, "Рівень 2: блокування"),
            new DifficultyOption(DifficultyLevel.Winning, "Рівень 3: виграш → блокування → хід")
        ];

        NewGameCommand = new RelayCommand(_ => StartNewGame());
        StartNewGame();
    }

    public ObservableCollection<CellViewModel> Cells { get; }
    public IReadOnlyList<DifficultyOption> DifficultyOptions { get; }
    public ICommand NewGameCommand { get; }

    public int BoardSize => TicTacToeGame.Size;

    public DifficultyLevel Difficulty
    {
        get => _difficulty;
        set
        {
            if (_difficulty == value) return;
            _difficulty = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    internal CellState GetSymbol(int row, int column) => _game[row, column];

    internal bool CanClickCell(int row, int column) =>
        _game.CanMakeMove(row, column, TicTacToeGame.HumanSymbol);

    internal void HandleCellClick(CellViewModel cell)
    {
        if (!_game.MakeMove(cell.Row, cell.Column, TicTacToeGame.HumanSymbol))
            return;

        RefreshBoard();
        PublishStatus();

        if (_game.Status == GameStatus.InProgress)
            PlayComputerMove();
    }

    private void PlayComputerMove()
    {
        (int Row, int Column)? move = _game.ChooseComputerMove(Difficulty);
        if (move is null) return;

        if (_game.MakeMove(move.Value.Row, move.Value.Column, TicTacToeGame.ComputerSymbol))
        {
            RefreshBoard();
            PublishStatus();
        }
    }

    private void StartNewGame()
    {
        _game.Reset();
        RefreshBoard();
        StatusMessage = HumanTurnText;
    }

    private void RefreshBoard()
    {
        foreach (CellViewModel cell in Cells)
            cell.Refresh();
    }

    private void PublishStatus()
    {
        StatusMessage = _game.Status switch
        {
            GameStatus.HumanWon => $"Ви перемогли! {NewGameHint}",
            GameStatus.ComputerWon => $"Комп'ютер переміг. {NewGameHint}",
            GameStatus.Draw => $"Нічия. {NewGameHint}",
            _ => _game.CurrentTurn == TicTacToeGame.HumanSymbol
                ? HumanTurnText
                : ComputerTurnText
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}