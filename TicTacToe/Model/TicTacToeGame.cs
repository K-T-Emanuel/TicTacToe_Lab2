using System;
using System.Collections.Generic;

namespace TicTacToe.Model
{
    public sealed class TicTacToeGame
    {
        public const int Size = 3;
        public const CellState HumanSymbol = CellState.X;
        public const CellState ComputerSymbol = CellState.O;

        private readonly CellState[,] _board = new CellState[Size, Size];

        public TicTacToeGame() => Reset();

        public GameStatus Status { get; private set; } = GameStatus.InProgress;

        public CellState CurrentTurn { get; private set; } = HumanSymbol;

        public CellState this[int row, int column] => _board[row, column];

        public void Reset()
        {
            for (int row = 0; row < Size; row++)
                for (int column = 0; column < Size; column++)
                    _board[row, column] = CellState.Empty;

            Status = GameStatus.InProgress;
            CurrentTurn = HumanSymbol;
        }

        public static bool IsInside(int row, int column) =>
            row >= 0 && row < Size && column >= 0 && column < Size;

        public bool IsEmpty(int row, int column) =>
            _board[row, column] == CellState.Empty;

        public bool CanMakeMove(int row, int column, CellState symbol) =>
            IsInside(row, column)
            && symbol != CellState.Empty
            && Status == GameStatus.InProgress
            && symbol == CurrentTurn
            && IsEmpty(row, column);

        public bool MakeMove(int row, int column, CellState symbol)
        {
            if (symbol == CellState.Empty)
                throw new ArgumentException("Символ ходу не може бути Empty.", nameof(symbol));

            if (!CanMakeMove(row, column, symbol))
                return false;

            _board[row, column] = symbol;
            Status = CheckWinner();

            if (Status == GameStatus.InProgress)
            {
                CurrentTurn = symbol == HumanSymbol ? ComputerSymbol : HumanSymbol;
            }

            return true;
        }

        public List<(int Row, int Column)> GetAvailableMoves()
        {
            List<(int Row, int Column)> moves = [];
            for (int row = 0; row < Size; row++)
                for (int column = 0; column < Size; column++)
                    if (_board[row, column] == CellState.Empty)
                        moves.Add((row, column));

            return moves;
        }

        public CellState? GetWinner()
        {
            if (HasFullLine(HumanSymbol)) return HumanSymbol;
            if (HasFullLine(ComputerSymbol)) return ComputerSymbol;
            return null;
        }

        public GameStatus CheckWinner()
        {
            CellState? winner = GetWinner();
            if (winner == HumanSymbol) return GameStatus.HumanWon;
            if (winner == ComputerSymbol) return GameStatus.ComputerWon;
            return IsBoardFull() ? GameStatus.Draw : GameStatus.InProgress;
        }

        public bool IsDraw() => IsBoardFull() && GetWinner() is null;

        public (int Row, int Column)? ChooseComputerMove(DifficultyLevel level)
        {
            if (Status != GameStatus.InProgress || CurrentTurn != ComputerSymbol)
                return null;

            List<(int Row, int Column)> available = GetAvailableMoves();
            if (available.Count == 0)
                return null;

            return level switch
            {
                DifficultyLevel.Random => PickRandom(available),
                DifficultyLevel.Blocking => PickBlocking(available),
                DifficultyLevel.Winning => PickWinning(available),
                _ => PickRandom(available)
            };
        }

        // --- Внутрішнє ---

        private bool IsBoardFull()
        {
            for (int row = 0; row < Size; row++)
                for (int column = 0; column < Size; column++)
                    if (_board[row, column] == CellState.Empty)
                        return false;

            return true;
        }

        private static (int Row, int Column) PickRandom(List<(int Row, int Column)> moves) =>
            moves[Random.Shared.Next(moves.Count)];

        private (int Row, int Column) PickBlocking(List<(int Row, int Column)> moves) =>
            FindWinningMove(HumanSymbol, moves) ?? PickRandom(moves);

        private (int Row, int Column) PickWinning(List<(int Row, int Column)> moves) =>
            FindWinningMove(ComputerSymbol, moves)
            ?? FindWinningMove(HumanSymbol, moves)
            ?? PickRandom(moves);

        private (int Row, int Column)? FindWinningMove(
            CellState symbol, List<(int Row, int Column)> moves)
        {
            foreach ((int row, int column) in moves)
            {
                _board[row, column] = symbol;
                bool wins = HasFullLine(symbol);
                _board[row, column] = CellState.Empty;
                if (wins) return (row, column);
            }

            return null;
        }

        private bool HasFullLine(CellState symbol)
        {
            for (int row = 0; row < Size; row++)
                if (IsFullLine(row, 0, 0, 1, symbol)) return true;

            for (int column = 0; column < Size; column++)
                if (IsFullLine(0, column, 1, 0, symbol)) return true;

            return IsFullLine(0, 0, 1, 1, symbol)
                || IsFullLine(0, Size - 1, 1, -1, symbol);
        }

        private bool IsFullLine(
            int startRow, int startColumn, int deltaRow, int deltaColumn, CellState symbol)
        {
            for (int step = 0; step < Size; step++)
            {
                int row = startRow + step * deltaRow;
                int column = startColumn + step * deltaColumn;
                if (_board[row, column] != symbol) return false;
            }

            return true;
        }
    }
}
