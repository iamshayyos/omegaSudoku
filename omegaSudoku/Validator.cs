using System;
using System.Collections.Generic;

namespace omegaSudoku
{
    public class Validator
    {
        private readonly int _maxSize = 25;
        private readonly int _minSize = 4;

        public bool IsValidFormat(string input)
        {
            int boardLen = (int)Math.Sqrt(input.Length);
            return boardLen * boardLen == input.Length && boardLen >= _minSize && boardLen <= _maxSize;
        }

        public bool IsBoardValid(int[,] board, int size)
        {
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

        private bool IsUnitValid(int[,] board, int size, int index, bool isRow)
        {
            HashSet<int> seen = new HashSet<int>();
            for (int i = 0; i < size; i++)
            {
                int num = isRow ? board[index, i] : board[i, index];
                if (num != 0 && !seen.Add(num))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsSubgridValid(int[,] board, int startRow, int startCol, int subgridSize)
        {
            HashSet<int> seen = new HashSet<int>();
            for (int row = 0; row < subgridSize; row++)
            {
                for (int col = 0; col < subgridSize; col++)
                {
                    int num = board[startRow + row, startCol + col];
                    if (num != 0 && !seen.Add(num))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsSolvable(int[,] board, int size)
        {
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    int num = board[row, col];
                    if (num != 0)
                    {
                        board[row, col] = 0;
                        if (!IsMoveValid(board, row, col, num, size))
                        {
                            board[row, col] = num;
                            return false;
                        }
                        board[row, col] = num;
                    }
                }
            }
            return true;
        }

        private bool IsMoveValid(int[,] board, int row, int col, int num, int size)
        {
            return IsUnitValidWithNum(board, row, col, num, true) &&
                   IsUnitValidWithNum(board, row, col, num, false) &&
                   IsSubgridValidWithNum(board, row, col, num, size);
        }

        private bool IsUnitValidWithNum(int[,] board, int row, int col, int num, bool isRow)
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                int value = isRow ? board[row, i] : board[i, col];
                if (value == num) return false;
            }
            return true;
        }

        private bool IsSubgridValidWithNum(int[,] board, int row, int col, int num, int size)
        {
            int subgridSize = (int)Math.Sqrt(size);
            int startRow = (row / subgridSize) * subgridSize;
            int startCol = (col / subgridSize) * subgridSize;

            for (int r = startRow; r < startRow + subgridSize; r++)
            {
                for (int c = startCol; c < startCol + subgridSize; c++)
                {
                    if (board[r, c] == num) return false;
                }
            }
            return true;
        }
    }
}
