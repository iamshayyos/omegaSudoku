using System;
using System.Collections.Generic;
using System.Numerics;

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
        public List<Cell> EmptyCells { get; set; }  // Allows backup and restore in recursion 
        public SolverState(int size)
        {
            Size = size;
            SubSize = (int)Math.Sqrt(size);
            FullMask = (1 << size) - 1;
            RowUsed = new int[size];
            ColUsed = new int[size];
            BoxUsed = new int[size];
            EmptyCells = new List<Cell>();
        }

        public int GetBoxIndex(int row, int col)
        {
            return (row / SubSize) * SubSize + (col / SubSize);
        }
    }

    public struct Cell
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Options { get; set; }

        public Cell(int row, int col, int options)
        {
            Row = row;
            Col = col;
            Options = options;
        }
    }
}
