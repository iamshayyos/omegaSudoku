using System;
using System.Collections.Generic;

namespace omegaSudoku
{
    public class SudokuSolver
    {
        public bool Solve(int[,] board, int size)
        {
            var rows = new int[size];
            var cols = new int[size];
            var subgrids = new int[size];

            InitializeBitmasks(board, size, rows, cols, subgrids);

            return BacktrackWithHeuristics(board, size, rows, cols, subgrids);
        }

        private void InitializeBitmasks(int[,] board, int size, int[] rows, int[] cols, int[] subgrids)
        {
            int subgridSize = (int)Math.Sqrt(size);
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    int value = board[r, c];
                    if (value != 0)
                    {
                        int bit = 1 << (value - 1);
                        rows[r] |= bit;
                        cols[c] |= bit;
                        subgrids[GetSubgridIndex(r, c, subgridSize)] |= bit;
                    }
                }
            }
        }

        private bool BacktrackWithHeuristics(int[,] board, int size, int[] rows, int[] cols, int[] subgrids)
        {
            (int row, int col)? nextCell = GetCellWithFewestCandidates(board, size, rows, cols, subgrids);

            if (nextCell == null) return true;

            int rowIndex = nextCell.Value.row;
            int colIndex = nextCell.Value.col;
            int subgridIndex = GetSubgridIndex(rowIndex, colIndex, (int)Math.Sqrt(size));

            int availableValues = GetAvailableValues(rows[rowIndex], cols[colIndex], subgrids[subgridIndex], size);

            for (int bit = 1; bit <= size; bit++)
            {
                if ((availableValues & (1 << (bit - 1))) != 0)
                {
                    board[rowIndex, colIndex] = bit;
                    rows[rowIndex] |= (1 << (bit - 1));
                    cols[colIndex] |= (1 << (bit - 1));
                    subgrids[subgridIndex] |= (1 << (bit - 1));

                    if (BacktrackWithHeuristics(board, size, rows, cols, subgrids))
                        return true;

                    board[rowIndex, colIndex] = 0;
                    rows[rowIndex] &= ~(1 << (bit - 1));
                    cols[colIndex] &= ~(1 << (bit - 1));
                    subgrids[subgridIndex] &= ~(1 << (bit - 1));
                }
            }

            return false;
        }

        private (int, int)? GetCellWithFewestCandidates(int[,] board, int size, int[] rows, int[] cols, int[] subgrids)
        {
            int minCandidates = int.MaxValue;
            (int, int)? bestCell = null;

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (board[r, c] != 0) continue;

                    int subgridIndex = GetSubgridIndex(r, c, (int)Math.Sqrt(size));
                    int availableValues = GetAvailableValues(rows[r], cols[c], subgrids[subgridIndex], size);
                    int numCandidates = CountBits(availableValues);

                    if (numCandidates < minCandidates)
                    {
                        minCandidates = numCandidates;
                        bestCell = (r, c);

                        if (numCandidates == 1) return bestCell;
                    }
                }
            }

            return bestCell;
        }

        private int GetAvailableValues(int rowMask, int colMask, int subgridMask, int size)
        {
            int usedValues = rowMask | colMask | subgridMask;
            return ~usedValues & ((1 << size) - 1);
        }

        private int CountBits(int value)
        {
            int count = 0;
            while (value > 0)
            {
                count += value & 1;
                value >>= 1;
            }
            return count;
        }

        private int GetSubgridIndex(int row, int col, int subgridSize)
        {
            return (row / subgridSize) * subgridSize + (col / subgridSize);
        }
    }
}
