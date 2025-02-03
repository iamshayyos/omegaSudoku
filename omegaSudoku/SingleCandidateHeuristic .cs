using omegaSudoku;
using System;
using System.Numerics; 

namespace OmegaSudoku
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
                        // If there is exactly one option, we will set it
                        if (possible != 0 && (possible & (possible - 1)) == 0)
                        {
                            int bit = possible;
                            int val = BitUtils.PopCount(possible);
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
