using omegaSudoku.Board;
using omegaSudoku.Exceptions;
using omegaSudoku.Interfaces;
using System;
using System.Collections.Generic;

namespace omegaSudoku.CoreLogic
{
    public class Validator : IValidator
    {
        // Minimum and maximum allowed board sizes.
        private readonly int _maxSize = 25;
        private readonly int _minSize = 1;

        /// <summary>
        /// Checks if the input string is in a valid format for a Sudoku board.
        /// It calculates the board size, verifies that the input length is a perfect square,
        /// and confirms that all characters are allowed.
        /// </summary>
        public bool IsValidFormat(string input, out int boardSize)
        {
            boardSize = (int)Math.Sqrt(input.Length);

            if (boardSize < _minSize)
            {
                throw new InputTooShortException(
                    $"Input length ({input.Length}) is too short. Must be between {_minSize} and {_maxSize}."
                );
            }
            if (boardSize > _maxSize)
            {
                throw new InputTooLargeException(
                    $"Input length ({input.Length}) is too large. Must be between {_minSize} and {_maxSize}."
                );
            }
            // Validate that each character is allowed for this board size
            foreach (char c in input)
            {
                if (!isValidChar(boardSize, c))
                {
                    throw new InvalidCharacterException(c);
                }
            }
            // Ensure the input length is a perfect square (required for a valid board)
            if (boardSize * boardSize != input.Length)
            {
                throw new InvalidFormatException(
                    $"Input length ({input.Length}) is not a perfect square for a board size in range [{_minSize}..{_maxSize}]."
                );
            }

            return true;
        }

        /// <summary>
        /// Validates the board by ensuring there are no duplicate numbers in any row, column, or subgrid.
        /// </summary>
        public bool IsBoardValid(SudokuBoard board, int size)
        {
            // Check each row and column for duplicates.
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
            // Check each subgrid for duplicates.
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

        /// <summary>
        /// Determines if a given character is valid based on the board size.
        /// For example, for a 9x9 board, allowed characters are '0' to '9'.
        /// </summary>
        private bool isValidChar(int boardSize, char cell)
        {
            char maxLetter = (char)('0' + boardSize);
            return (cell >= '0') && (cell <= maxLetter);
        }

        /// <summary>
        /// Checks if a row or column (depending on the isRow flag) contains no duplicate non-zero numbers.
        /// </summary>
        private bool IsUnitValid(SudokuBoard board, int size, int index, bool isRow)
        {
            HashSet<int> seen = new HashSet<int>();
            for (int i = 0; i < size; i++)
            {
                int num = isRow ? board.Board[index, i] : board.Board[i, index];
                if (num != 0)
                {
                    if (!seen.Add(num))
                        return false; // Duplicate found
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if a subgrid  contains no duplicate numbers.
        /// </summary>
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
                        return false; // Duplicate found
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Determines if the board is solvable by verifying that each cell
        /// can be legally placed without contradicting Sudoku rules.
        /// </summary>
        public bool IsSolvable(SudokuBoard board, int size)
        {
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    int num = board.Board[row, col];
                    if (num != 0)
                    {
                        // Temporarily remove the number for validation.
                        board.Board[row, col] = 0;
                        if (!IsMoveValid(board, row, col, num, size))
                        {
                            // Restore the number and throw an exception if a contradiction is found.
                            board.Board[row, col] = num;
                            throw new UnsolvableBoardException(
                                $"Contradiction found with value '{num}' at position ({row},{col})."
                            );
                        }
                        // Restore the number after validation.
                        board.Board[row, col] = num;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if placing a number in the specified cell is valid according to Sudoku rules.
        /// This includes checking the row, column, and corresponding subgrid.
        /// </summary>
        private bool IsMoveValid(SudokuBoard board, int row, int col, int num, int size)
        {
            // Validate the row and column.
            for (int i = 0; i < size; i++)
            {
                if (board.Board[row, i] == num) return false;
                if (board.Board[i, col] == num) return false;
            }

            int subgridSize = (int)Math.Sqrt(size);
            int startRow = row / subgridSize * subgridSize;
            int startCol = col / subgridSize * subgridSize;
            // Validate the subgrid.
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
