using omegaSudoku;
using System;
using System.Numerics;

namespace omegaSudoku
{
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
            SolverState state = new SolverState(size);

            // Initialize masks based on the board values.
            if (!InitializeState(board, state))
            {
                return false;
            }

            // Apply heuristics (constraint propagation) until no further progress can be made.
            bool progress;
            do
            {
                progress = false;
                foreach (var heuristic in _heuristics)
                {
                    if (heuristic.Apply(board, state))
                    {
                        progress = true;
                    }
                }
            } while (progress);

            // Begin backtracking with optimized cell selection (MRV).
            return BacktrackOptimized(board, state);
        }

        private bool InitializeState(SudokuBoard board, SolverState state)
        {
            for (int r = 0; r < state.Size; r++)
            {
                for (int c = 0; c < state.Size; c++)
                {
                    int val = board.Board[r, c];
                    if (val != 0)
                    {
                        int bit = 1 << (val - 1);
                        int boxIndex = state.GetBoxIndex(r, c);
                        // Check if the value is already used in the row, column, or box.
                        if ((state.RowUsed[r] & bit) != 0 ||
                            (state.ColUsed[c] & bit) != 0 ||
                            (state.BoxUsed[boxIndex] & bit) != 0)
                        {
                            return false; // Conflict detected.
                        }
                        state.RowUsed[r] |= bit;
                        state.ColUsed[c] |= bit;
                        state.BoxUsed[boxIndex] |= bit;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Optimized backtracking using the MRV heuristic: selects the empty cell with the fewest candidate numbers.
        /// </summary>
        private bool BacktrackOptimized(SudokuBoard board, SolverState state)
        {
            int bestRow = -1, bestCol = -1;
            int bestCandidateCount = int.MaxValue;
            int bestCandidates = 0;

            // Find the empty cell with the minimum number of candidates.
            for (int r = 0; r < state.Size; r++)
            {
                for (int c = 0; c < state.Size; c++)
                {
                    if (board.Board[r, c] == 0)
                    {
                        int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                        int candidates = state.FullMask & ~used;
                        int count = BitOperations.PopCount((uint)candidates);
                        if (count == 0)
                            return false; // No candidates available; backtrack.
                        if (count < bestCandidateCount)
                        {
                            bestCandidateCount = count;
                            bestCandidates = candidates;
                            bestRow = r;
                            bestCol = c;
                            if (count == 1) // Optimal: only one candidate.
                                break;
                        }
                    }
                }
            }

            // If no empty cell is found, the board is solved.
            if (bestRow == -1)
                return true;

            // Try each candidate for the selected cell.
            while (bestCandidates != 0)
            {
                int bit = bestCandidates & -bestCandidates; // Get the lowest set bit.
                bestCandidates -= bit;
                int val = BitOperations.TrailingZeroCount((uint)bit) + 1;

                // Assign the candidate and update masks.
                board.Board[bestRow, bestCol] = val;
                state.RowUsed[bestRow] |= bit;
                state.ColUsed[bestCol] |= bit;
                state.BoxUsed[state.GetBoxIndex(bestRow, bestCol)] |= bit;

                if (BacktrackOptimized(board, state))
                    return true;

                // Undo the assignment (backtracking).
                board.Board[bestRow, bestCol] = 0;
                state.RowUsed[bestRow] &= ~bit;
                state.ColUsed[bestCol] &= ~bit;
                state.BoxUsed[state.GetBoxIndex(bestRow, bestCol)] &= ~bit;
            }

            return false;
        }
    }
}
