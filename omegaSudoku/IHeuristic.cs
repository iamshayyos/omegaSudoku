using omegaSudoku;

namespace omegaSudoku
{
    public interface IHeuristic
    {
        /// <summary>
        /// Trying to apply the heuristic to the board and the current state of the solution.
        /// Returns true if changes have been made.
        /// </summary>
        bool Apply(SudokuBoard board, SolverState state);
    }
}
