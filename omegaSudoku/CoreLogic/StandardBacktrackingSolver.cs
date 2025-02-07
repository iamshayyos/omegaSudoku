using omegaSudoku.BoardAndCells;
using omegaSudoku.Interfaces;
using System.Numerics;

namespace omegaSudoku.CoreLogic
{
    /// <summary>
    /// Solver for standard Sudoku boards (e.g., 9x9, 16x16).
    /// Uses heuristics, backtracking, forward checking, and transposition tables.
    /// </summary>
    public class StandardBacktrackingSolver : BaseBacktrackingSolver
    {
        public StandardBacktrackingSolver(IHeuristic[] heuristics)
            : base(heuristics)
        {
        }

        public override bool Solve(SudokuBoard board)
        {
            SolverState state = new SolverState(board.Size);
            if (!InitializeState(board, state))
                return false;

            // 1. Apply heuristics repeatedly until no more progress
            bool progress;
            do
            {
                progress = false;
                foreach (var heuristic in _heuristics)
                {
                    if (heuristic.Apply(board, state))
                    {
                        if (!_constraintChecker.ForwardCheck(board, state))
                            return false;
                        progress = true;
                    }
                }
            } while (progress);

            // 2. Use backtracking with Minimum Remaining Values (MRV)
            return BacktrackOptimized(board, state);
        }

        private bool BacktrackOptimized(SudokuBoard board, SolverState state)
        {
            // If configuration has been seen before, stop
            if (AlreadySeenConfiguration(board))
                return false;

            int bestRow = -1, bestCol = -1;
            int bestCandidateCount = int.MaxValue;
            int bestCandidates = 0;

            // 1. Choose the cell with the fewest possible candidates (MRV heuristic)
            for (int r = 0; r < state.Size; r++)
            {
                for (int c = 0; c < state.Size; c++)
                {
                    if (board.Board[r, c] == 0)
                    {
                        int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                        int candidates = state.FullMask & ~used;
                        int count = BitOperations.PopCount((uint)candidates);
                        if (count == 0) return false; // No candidates available -> failure
                        if (count < bestCandidateCount)
                        {
                            bestCandidateCount = count;
                            bestCandidates = candidates;
                            bestRow = r;
                            bestCol = c;
                            if (count == 1)
                                break;
                        }
                    }
                }
                if (bestCandidateCount == 1) // Exit loop if a single candidate is found
                    break;
            }

            // If no empty cell is found, the puzzle is solved
            if (bestRow == -1)
                return true;

            // 2. Try each candidate in the chosen cell
            while (bestCandidates != 0)
            {
                int bit = bestCandidates & -bestCandidates;  // Extract the lowest bit
                bestCandidates -= bit;
                int val = BitOperations.TrailingZeroCount((uint)bit) + 1;

                // Assign the value
                board.Board[bestRow, bestCol] = val;
                state.RowUsed[bestRow] |= bit;
                state.ColUsed[bestCol] |= bit;
                state.BoxUsed[state.GetBoxIndex(bestRow, bestCol)] |= bit;

                // Perform forward checking
                if (_constraintChecker.ForwardCheck(board, state))
                {
                    // Proceed with recursion only if valid
                    if (BacktrackOptimized(board, state))
                        return true;
                }

                // Undo assignment
                board.Board[bestRow, bestCol] = 0;
                state.RowUsed[bestRow] &= ~bit;
                state.ColUsed[bestCol] &= ~bit;
                state.BoxUsed[state.GetBoxIndex(bestRow, bestCol)] &= ~bit;
            }

            return false;
        }
    }
}
