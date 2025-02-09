using omegaSudoku.BoardAndCells;
using omegaSudoku.Exceptions;
using omegaSudoku.Interfaces;
using System;
using System.Collections.Generic;

namespace omegaSudoku.CoreLogic
{
    public class Validator : IValidator
    {   
        private readonly int _maxSize = 25;
        private readonly int _minSize = 1;

        public bool IsValidFormat(string input, out int boardSize)
        {
            boardSize = (int)Math.Sqrt(input.Length);
            
            // Check if too small
            if (boardSize < _minSize)
            {
                throw new InputTooShortException(
                    $"Input length ({input.Length}) is too short. Must be between {_minSize} and {_maxSize}."
                );
            }
            // Check if too large
            if (boardSize > _maxSize) 
            {
                throw new InputTooLargeException(
                    $"Input length ({input.Length}) is too large. Must be between {_minSize} and {_maxSize}."
                );
            }
            if (boardSize * boardSize != input.Length) 
            {
                throw new InvalidFormatException(
                    $"Input length ({input.Length}) is not a perfect square for a board size in range [{_minSize}..{_maxSize}]."
                );
            }
            // Validate characters against allowed set
            foreach (char c in input)
            {
                if (!isValidChar(boardSize,c))
                {
                    throw new InvalidCharacterException(c);
                }
            }
            return true;
        }

        public bool IsBoardValid(SudokuBoard board, int size)
        {
            // Check rows and columns.
            for (int i = 0; i < size; i++)
            {
                if (!IsUnitValid(board, size, i, isRow: true))
                {
                    throw new BoardInitializationException(
                        $"Duplicate value found in row {i + 1}."
                    );
                }
                if (!IsUnitValid(board, size, i, isRow: false))
                {
                    throw new BoardInitializationException(
                        $"Duplicate value found in column {i + 1}."
                    );
                }
            }

            int subgridSize = (int)Math.Sqrt(size);
            for (int row = 0; row < size; row += subgridSize)
            {
                for (int col = 0; col < size; col += subgridSize)
                {
                    if (!IsSubgridValid(board, row, col, subgridSize))
                    {
                        throw new BoardInitializationException(
                            $"Duplicate values found in subgrid starting at ({row},{col})."
                        );
                    }
                }
            }

            return true;
        }

        private bool isValidChar(int boardSize, char cell)
        {
            char maxLetter =(char)('0'+boardSize);
            return (maxLetter >= cell)&&(cell>='0');
        }

        private bool IsUnitValid(SudokuBoard board, int size, int index, bool isRow)
        {
            HashSet<int> seen = new HashSet<int>();
            for (int i = 0; i < size; i++)
            {
                int num = isRow ? board.Board[index, i] : board.Board[i, index];
                if (num != 0)
                {
                    if (!seen.Add(num))
                        return false;
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
            // For each non-zero cell, check that its value can be legally placed.
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    int num = board.Board[row, col];
                    if (num != 0)
                    {
                        // Temporarily remove the number to validate its placement
                        board.Board[row, col] = 0;
                        if (!IsMoveValid(board, row, col, num, size))
                        {
                            // Restore the number before throwing the exception
                            board.Board[row, col] = num;
                            throw new UnsolvableBoardException(
                                $"Contradiction found with value '{num}' at position ({row},{col})."
                            );
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
            int startRow = row / subgridSize * subgridSize;
            int startCol = col / subgridSize * subgridSize;
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
