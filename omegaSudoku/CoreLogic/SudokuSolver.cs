using omegaSudoku.Interfaces;

namespace omegaSudoku.CoreLogic
{
    /// <summary>
    /// Uses FastBacktrackingSolver for boards up to 16x16,
    /// FastAdvanced25x25Solver for 25x25 boards,
    /// and Advanced25x25Solver (if needed) for additional strategies.
    /// </summary>
    public class SudokuSolver : ISudokuSolver
    {
        private readonly IHeuristic[] _heuristics;

        public SudokuSolver(IHeuristic[] heuristics)
        {
            _heuristics = heuristics;
        }

        public bool Solve(SudokuBoard board)
        {
            int size = board.Size;
            if (size <= 16)
            {
                var solver = new FastBacktrackingSolver();
                return solver.Solve(board);
            }
            else if (size == 25)
            {
                var solver = new FastAdvanced25x25Solver();
                return solver.Solve(board);
            }
            return false;
        }
    }
}
