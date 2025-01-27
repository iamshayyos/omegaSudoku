using System;
using System.Collections.Generic;

namespace omegaSudoku
{
    public class Validator
    {
        private readonly int _maxSize = 25;
        private readonly int _minSize = 1;

        public bool IsValidFormat(string input)
        {
            //check for a valid length min vlue is 1 and max value is 25
            int boardLen = (int)Math.Sqrt(input.Length);
            return boardLen * boardLen == input.Length 
                   && boardLen >= _minSize 
                   && boardLen <= _maxSize;
        }

        public bool IsBoardValid(int[,] board, int size)
        {
            // base check: no duplicates in rows, columns or subgrids
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
                if (num != 0)
                {
                    if (!seen.Add(num)) return false;
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
            

            //To check for a collision, we'll try to make sure that every existing digit can remain valid
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
            //check row
            for (int i = 0; i < size; i++)
            {
                if (board[row, i] == num) return false;
            }
            //check column
            for (int i = 0; i < size; i++)
            {
                if (board[i, col] == num) return false;
            }
            //check subgrid
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
