using System;

namespace omegaSudoku
{
    public class SudokuSolver
    {
        public bool Solve(int[,] board, int size)
        {
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (board[row, col] == 0)
                    {
                        for (int num = 1; num <= size; num++)
                        {
                            if (IsValidMove(board, row, col, num, size))
                            {
                                board[row, col] = num;
                                if (Solve(board, size)) return true;
                                board[row, col] = 0;
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        private bool IsValidMove(int[,] board, int row, int col, int num, int size)
        {
            for (int i = 0; i < size; i++)
            {
                if (board[row, i] == num || board[i, col] == num) return false;
            }

            int subgridSize = (int)Math.Sqrt(size);
            int startRow = row / subgridSize * subgridSize;
            int startCol = col / subgridSize * subgridSize;

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
