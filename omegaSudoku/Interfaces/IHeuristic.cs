using omegaSudoku.BoardAndCells;
using omegaSudoku.CoreLogic;

namespace omegaSudoku.Interfaces
{
    public interface IHeuristic
    {
        /// <summary>
        /// Attempts to apply the heuristic to the board and the current solver state.
        /// Returns true if any changes were made.
        /// </summary>
        bool Apply(SudokuBoard board, SolverState state);
    }
}
