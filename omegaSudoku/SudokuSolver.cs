using System;
using System.Collections.Generic;

namespace omegaSudoku
{
   
    public class SudokuSolver : ISudokuSolver
    {
        private int _size;
        private int _subSize;

        private int[] rowUsed;
        private int[] colUsed;
        private int[] boxUsed;
        private int fullMask;

        private List<(int row, int col)> emptyCells;
        private int[,] _board;

        public bool Solve(int[,] board, int size)
        {
            _board = board;
            _size = size;
            _subSize = (int)Math.Sqrt(size);

            rowUsed = new int[size];
            colUsed = new int[size];
            boxUsed = new int[size];

            fullMask = (1 << size) - 1;
            emptyCells = new List<(int, int)>();

            if (!InitializeMasks()) return false;
            BuildEmptyCellsList();

            return Backtrack();
        }

        //initialize the masks for the values that are already written on the board
        //if there is a conflict, return false
        private bool InitializeMasks()
        {
            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    int val = _board[r, c];
                    if (val != 0)
                    {
                        int bit = 1 << (val - 1);
                        int boxIndex = GetBoxIndex(r, c);

                        if ((rowUsed[r] & bit) != 0) return false;
                        if ((colUsed[c] & bit) != 0) return false;
                        if ((boxUsed[boxIndex] & bit) != 0) return false;

                        rowUsed[r] |= bit;
                        colUsed[c] |= bit;
                        boxUsed[boxIndex] |= bit;
                    }
                }
            }
            return true;
        }

        private void BuildEmptyCellsList()
        {
            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    if (_board[r, c] == 0)
                    {
                        emptyCells.Add((r, c));
                    }
                }
            }
        }

        private bool Backtrack()
        {
            //if there are no empty cells, the board is full return true
            if (emptyCells.Count == 0)
            {
                return true;
            }

            //search for the cell with the least amount of possible values(MRV)
            int bestIndex = -1;
            int bestMovesCount = _size + 1;

            for (int i = 0; i < emptyCells.Count; i++)
            {
                (int r, int c) = emptyCells[i];
                int usedBits = rowUsed[r] | colUsed[c] | boxUsed[GetBoxIndex(r, c)];
                int freeBits = fullMask & ~usedBits;
                int count = PopCount(freeBits);

                if (count < bestMovesCount)
                {
                    bestMovesCount = count;
                    bestIndex = i;
                    if (bestMovesCount <= 1) break;
                }
            }

            if (bestIndex == -1) return true;
            if (bestMovesCount == 0) return false;

            //replace the best cell with the last cell in the list for efficient removal
            (emptyCells[bestIndex], emptyCells[emptyCells.Count - 1]) =
                (emptyCells[emptyCells.Count - 1], emptyCells[bestIndex]);

            var cell = emptyCells[emptyCells.Count - 1];
            emptyCells.RemoveAt(emptyCells.Count - 1);

            int row = cell.row;
            int col = cell.col;
            int used = rowUsed[row] | colUsed[col] | boxUsed[GetBoxIndex(row, col)];
            int candidates = fullMask & ~used;

            while (candidates != 0)
            {
                int bit = candidates & (-candidates);
                candidates &= (candidates - 1);

                int val = BitToValue(bit);
                //insert the value to the board
                _board[row, col] = val;
                rowUsed[row] |= bit;
                colUsed[col] |= bit;
                boxUsed[GetBoxIndex(row, col)] |= bit;

                if (Backtrack()) return true;

                //remove the value from the board
                _board[row, col] = 0;
                rowUsed[row] &= ~bit;
                colUsed[col] &= ~bit;
                boxUsed[GetBoxIndex(row, col)] &= ~bit;
            }

            //insert the cell back to the list
            emptyCells.Add(cell);
            return false;
        }

        private int GetBoxIndex(int row, int col)
        {
            return (row / _subSize) * _subSize + (col / _subSize);
        }

        private int BitToValue(int bit)
        {
            int val = 0;
            while (bit > 0)
            {
                bit >>= 1;
                val++;
            }
            return val;
        }

        private int PopCount(int x)
        {
            //x counts the number of bits that are on
            int count = 0;
            while (x != 0)
            {
                x &= (x - 1);
                count++;
            }
            return count;
        }
    }
}
