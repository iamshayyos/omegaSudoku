using omegaSudoku.Board;
using omegaSudoku.CoreLogic;
using omegaSudoku.Interfaces;
using System;
using System.Collections.Generic;
using System.Numerics;
namespace omegaSudoku.Heuristics
{
    /// <summary>
    /// Implements the naked singles heuristic.
    /// For each empty cell, if it has exactly one candidate, assign that candidate immediately.
    /// Returns false if any cell is found to have zero candidates.
    /// </summary>
    public class NakedSinglesHeuristic : IHeuristic
    {
        public bool Apply(SudokuBoard board, SolverState state)
        {
            bool anyChange = false;
            int size = state.Size;
            int fullMask = state.FullMask;
            int subSize = state.SubSize;

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (board.Board[r, c] != 0)
                        continue;
                    int boxIdx = state.GetBoxIndex(r, c);
                    int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[boxIdx];
                    int candidates = fullMask & ~used;
                    int count = BitOperations.PopCount((uint)candidates);

                    if (count == 0)
                        return false; // unsolvable state
                    if (count == 1)
                    {
                        int bit = candidates & -candidates; // lowest set bit
                        int val = BitOperations.TrailingZeroCount((uint)bit) + 1;

                        board.Board[r, c] = val;
                        state.RowUsed[r] |= bit;
                        state.ColUsed[c] |= bit;
                        state.BoxUsed[boxIdx] |= bit;
                        anyChange = true;
                    }
                }
            }
            return anyChange;
        }
    }
}
