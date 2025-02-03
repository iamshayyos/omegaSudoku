namespace omegaSudoku
{
    public interface ISudokuSolver
    {
        /// <summary>
        /// Attempts to solve the given board. Returns true if a solution is found.
        /// </summary>
        bool Solve(SudokuBoard board);
    }
}
