using omegaSudoku;
using System;
using System.Collections.Generic;

namespace OmegaSudoku
{
    public class Validator : IValidator
    {
        private readonly int _maxSize = 25;
        private readonly int _minSize = 1;

        public bool IsValidFormat(string input, out int boardSize)
        {
            boardSize = (int)Math.Sqrt(input.Length);
            return boardSize * boardSize == input.Length
                   && boardSize >= _minSize
                   && boardSize <= _maxSize;
        }

        public bool IsBoardValid(SudokuBoard board, int size)
        {
            // Checking rows and columns
            for (int i = 0; i < size; i++)
            {
                if (!IsUnitValid(board, size, i, true) || !IsUnitValid(board, size, i, false))
                {
                    return false;
                }
            }

            int subgridSize = (int)Math.Sqrt(size);
            for (int row = 0; row < size; row += subgridSize)
            {
                for (int col = 0; col < size; col += subgridSize)
                {
                    if (!IsSubgridValid(board, row, col, subgridSize))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool IsUnitValid(SudokuBoard board, int size, int index, bool isRow)
        {
            HashSet<int> seen = new HashSet<int>();
            for (int i = 0; i < size; i++)
            {
                int num = isRow ? board.Board[index, i] : board.Board[i, index];
                if (num != 0)
                {
                    if (!seen.Add(num)) return false;
                }
            }
            return true;
        }

        private bool IsSubgridValid(SudokuBoard board, int startRow, int startCol, int subgridSize)
        {
            HashSet<int> seen = new HashSet<int>();
            for (int row = 0; row < subgridSize; row++)
            {
                for (int col = 0; col < subgridSize; col++)
                {
                    int num = board.Board[startRow + row, startCol + col];
                    if (num != 0 && !seen.Add(num))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsSolvable(SudokuBoard board, int size)
        {
            // For each existing value, we will check that it does not conflict with other options.
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    int num = board.Board[row, col];
                    if (num != 0)
                    {
                        board.Board[row, col] = 0;
                        if (!IsMoveValid(board, row, col, num, size))
                        {
                            board.Board[row, col] = num;
                            return false;
                        }
                        board.Board[row, col] = num;
                    }
                }
            }
            return true;
        }

        private bool IsMoveValid(SudokuBoard board, int row, int col, int num, int size)
        {
            for (int i = 0; i < size; i++)
            {
                if (board.Board[row, i] == num) return false;
                if (board.Board[i, col] == num) return false;
            }

            int subgridSize = (int)Math.Sqrt(size);
            int startRow = (row / subgridSize) * subgridSize;
            int startCol = (col / subgridSize) * subgridSize;
            for (int r = startRow; r < startRow + subgridSize; r++)
            {
                for (int c = startCol; c < startCol + subgridSize; c++)
                {
                    if (board.Board[r, c] == num) return false;
                }
            }
            return true;
        }
    }
}
