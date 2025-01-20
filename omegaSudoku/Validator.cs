using System;

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
                if (!IsRowValid(board, i, size) || !IsColumnValid(board, i, size))
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

        private bool IsRowValid(int[,] board, int row, int size)
        {
            bool[] seen = new bool[size + 1];
            for (int col = 0; col < size; col++)
            {
                int num = board[row, col];
                if (num != 0)
                {
                    if (seen[num]) return false;
                    seen[num] = true;
                }
            }
            return true;
        }

        private bool IsColumnValid(int[,] board, int col, int size)
        {
            bool[] seen = new bool[size + 1];
            for (int row = 0; row < size; row++)
            {
                int num = board[row, col];
                if (num != 0)
                {
                    if (seen[num]) return false;
                    seen[num] = true;
                }
            }
            return true;
        }

        private bool IsSubgridValid(int[,] board, int startRow, int startCol, int subgridSize)
        {
            bool[] seen = new bool[subgridSize * subgridSize + 1];
            for (int row = 0; row < subgridSize; row++)
            {
                for (int col = 0; col < subgridSize; col++)
                {
                    int num = board[startRow + row, startCol + col];
                    if (num != 0)
                    {
                        if (seen[num]) return false;
                        seen[num] = true;
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
                        if (!IsValidMove(board, row, col, num, size))
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

        private bool IsValidMove(int[,] board, int row, int col, int num, int size)
        {
            for (int i = 0; i < size; i++)
            {
                if (board[row, i] == num || board[i, col] == num)
                    return false;
            }

            int subgridSize = (int)Math.Sqrt(size);
            int startRow = row / subgridSize * subgridSize;
            int startCol = col / subgridSize * subgridSize;

            for (int r = startRow; r < startRow + subgridSize; r++)
            {
                for (int c = startCol; c < startCol + subgridSize; c++)
                {
                    if (board[r, c] == num)
                        return false;
                }
            }
            return true;
        }


    }
}
