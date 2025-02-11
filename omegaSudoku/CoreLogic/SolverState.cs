using omegaSudoku.Board;

namespace omegaSudoku.CoreLogic
{
    public class SolverState
    {
        public SudokuBoard Board { get; }
        public int Size { get; }
        public int SubSize { get; }
        public int FullMask { get; }
        public int[] RowUsed { get; }
        public int[] ColUsed { get; }
        public int[] BoxUsed { get; }

        public SolverState(SudokuBoard board, int size, int subSize, int fullMask,
                           int[] rowUsed, int[] colUsed, int[] boxUsed)
        {
            Board = board;
            Size = size;
            SubSize = subSize;
            FullMask = fullMask;
            RowUsed = rowUsed;
            ColUsed = colUsed;
            BoxUsed = boxUsed;
        }

        public int GetBoxIndex(int row, int col)
        {
            return (row / SubSize) * SubSize + (col / SubSize);
        }
    }
}
