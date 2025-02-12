using System.Collections.Generic;
using System.Numerics;
using omegaSudoku.Board;

namespace omegaSudoku.CoreLogic
{
    /// <summary>
    /// Solves Sudoku using MRV heuristic with backtracking.
    /// Periodically checks global constraints to prune unsolvable branches.
    /// </summary>
    public class MRVBacktrackingSolver
    {
        private readonly GlobalConstraintChecker _globalChecker;
        private readonly int _checkInterval;

        public MRVBacktrackingSolver(GlobalConstraintChecker checker, int checkInterval = 10)
        {
            _globalChecker = checker;
            _checkInterval = checkInterval;
        }

        /// <summary>
        /// Initializes solving process by collecting empty cells and starting recursion.
        /// </summary>
        public bool Solve(SolverState state)
        {
            List<(int r, int c)> empties = new List<(int, int)>();

            // Collect all empty cells
            for (int r = 0; r < state.Size; r++)
            {
                for (int c = 0; c < state.Size; c++)
                {
                    if (state.BoardArray[r, c] == 0)
                        empties.Add((r, c));
                }
            }

            return SolveRecursively(state, empties, 0);
        }

        /// <summary>
        /// Recursively assigns values to empty cells using MRV heuristic.
        /// Performs a global constraint check at regular intervals.
        /// </summary>
        private bool SolveRecursively(SolverState state, List<(int r, int c)> empties, int pos)
        {
            if (pos % _checkInterval == 0 && !_globalChecker.CheckGlobalConstraints(state))
                return false;

            if (pos == empties.Count) // Solution found
                return true;

            // MRV: Find the cell with the fewest candidates
            int minCandidates = int.MaxValue;
            int selectedIndex = pos;
            int candidateMask = 0;

            for (int i = pos; i < empties.Count; i++)
            {
                var (r, c) = empties[i];
                int candidates = state.GetCandidates(r, c);
                int count = BitOperations.PopCount((uint)candidates);
                if (count < minCandidates)
                {
                    minCandidates = count;
                    selectedIndex = i;
                    candidateMask = candidates;
                    if (minCandidates == 1)
                        break;
                }
            }

            if (minCandidates == 0) // No valid candidates, backtrack
                return false;

            // Swap the selected cell to the current position
            var tmp = empties[pos];
            empties[pos] = empties[selectedIndex];
            empties[selectedIndex] = tmp;

            var (row, col) = empties[pos];
            while (candidateMask != 0)
            {
                int bit = candidateMask & -candidateMask;
                candidateMask -= bit;

                state.SetDigit(row, col, bit);

                if (SolveRecursively(state, empties, pos + 1))
                    return true;

                state.UnsetDigit(row, col, bit); // Backtrack
            }

            return false;
        }
    }
}
