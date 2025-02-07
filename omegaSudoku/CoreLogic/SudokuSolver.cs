using omegaSudoku.BoardAndCells;
using omegaSudoku.Interfaces;

namespace omegaSudoku.CoreLogic
{
    /// <summary>
    /// Facade for solving a Sudoku board.
    /// Decides which solver strategy to use (standard vs advanced 25x25).
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
                // Solver for 9x9 or 16x16
                var solver = new StandardBacktrackingSolver(_heuristics);
                return solver.Solve(board);
            }
            else if (size == 25)
            {
                // Advanced solver for 25x25
                var solver = new Advanced25x25Solver(_heuristics);
                return solver.Solve(board);
            }

            // Does not support other board sizes
            return false;
        }
    }
}
