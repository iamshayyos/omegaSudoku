using omegaSudoku.Board;
using omegaSudoku.CoreLogic;
using omegaSudoku.Interfaces;
using System;
using System.Numerics;

namespace omegaSudoku.Heuristics
{
    public class SingleCandidateHeuristic : IHeuristic
    {
        public bool Apply(SudokuBoard board, SolverState state)
        {
            bool changed = false;
            for (int r = 0; r < state.Size; r++)
            {
                for (int c = 0; c < state.Size; c++)
                {
                    if (board.Board[r, c] == 0)
                    {
                        int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                        int possible = state.FullMask & ~used;
                        // If there is exactly one candidate, fill it in.
                        if (possible != 0 && (possible & possible - 1) == 0)
                        {
                            int bit = possible;
                            int val = BitOperations.TrailingZeroCount((uint)possible) + 1;
                            board.Board[r, c] = val;
                            state.RowUsed[r] |= bit;
                            state.ColUsed[c] |= bit;
                            state.BoxUsed[state.GetBoxIndex(r, c)] |= bit;
                            changed = true;
                        }
                    }
                }
            }
            return changed;
        }
    }
}
