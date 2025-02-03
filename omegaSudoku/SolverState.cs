using System;

namespace omegaSudoku
{
    public class SolverState
    {
        public int Size { get; private set; }
        public int SubSize { get; private set; }
        public int FullMask { get; private set; }
        public int[] RowUsed { get; private set; }
        public int[] ColUsed { get; private set; }
        public int[] BoxUsed { get; private set; }

        public SolverState(int size)
        {
            Size = size;
            SubSize = (int)Math.Sqrt(size);
            FullMask = (1 << size) - 1;
            RowUsed = new int[size];
            ColUsed = new int[size];
            BoxUsed = new int[size];
        }

        public int GetBoxIndex(int row, int col)
        {
            return (row / SubSize) * SubSize + (col / SubSize);
        }
    }
}
