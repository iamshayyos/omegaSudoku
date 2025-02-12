using System;
using omegaSudoku.Board;

namespace omegaSudoku.CoreLogic
{
    public class SolverState
    {
        public int Size { get; }
        public int SubSize { get; }
        public int FullMask { get; }

        public int[] RowUsed { get; }
        public int[] ColUsed { get; }
        public int[] BoxUsed { get; }
        public int[,] BoardArray { get; }

        public SolverState(SudokuBoard sudokuBoard)
        {
            Size = sudokuBoard.Size;
            SubSize = (int)Math.Sqrt(Size);
            FullMask = (1 << Size) - 1;

            BoardArray = sudokuBoard.Board; 

            RowUsed = new int[Size];
            ColUsed = new int[Size];
            BoxUsed = new int[Size];

            // Initialize constraints (rowUsed, colUsed, boxUsed) based on the given board
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    int val = BoardArray[r, c];
                    if (val != 0)
                    {
                        int bit = 1 << (val - 1);
                        RowUsed[r] |= bit;
                        ColUsed[c] |= bit;
                        int boxIndex = GetBoxIndex(r, c);
                        BoxUsed[boxIndex] |= bit;
                    }
                }
            }
        }

        public int GetBoxIndex(int row, int col)
        {
            return (row / SubSize) * SubSize + (col / SubSize);
        }

        /// <summary>
        /// Returns a bitmask representing the possible values for the given cell.
        /// </summary>
        public int GetCandidates(int row, int col)
        {
            int boxIdx = GetBoxIndex(row, col);
            int used = RowUsed[row] | ColUsed[col] | BoxUsed[boxIdx];
            return FullMask & ~used;
        }

        /// <summary>
        /// Assigns a digit (bit) to the specified cell (row, col).
        /// </summary>
        public void SetDigit(int row, int col, int bit)
        {
            BoardArray[row, col] = BitToValue(bit);
            RowUsed[row] |= bit;
            ColUsed[col] |= bit;
            BoxUsed[GetBoxIndex(row, col)] |= bit;
        }

        /// <summary>
        /// Removes a digit (bit) from the specified cell (row, col).
        /// </summary>
        public void UnsetDigit(int row, int col, int bit)
        {
            BoardArray[row, col] = 0;
            RowUsed[row] &= ~bit;
            ColUsed[col] &= ~bit;
            BoxUsed[GetBoxIndex(row, col)] &= ~bit;
        }

        /// <summary>
        /// Converts a bit representation to its numerical value (1-based).
        /// </summary>
        private int BitToValue(int bit)
        {
            return (int)Math.Log2(bit) + 1;
            // Alternatively, for better performance: BitOperations.TrailingZeroCount((uint)bit) + 1
        }
    }
}
