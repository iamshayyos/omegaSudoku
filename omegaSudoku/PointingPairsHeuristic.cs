using System;
using System.Numerics;

namespace omegaSudoku
{
    /// <summary>
    /// Implements a simple Pointing Pairs heuristic.
    /// For each subgrid (box), if a candidate appears only in one row or column within the box,
    /// eliminate that candidate from the rest of that row or column outside the box.
    /// </summary>
    public class PointingPairsHeuristic : IHeuristic
    {
        public bool Apply(SudokuBoard board, SolverState state)
        {
            bool changed = false;
            int size = state.Size;
            int subSize = state.SubSize;
            // Process each subgrid.
            for (int br = 0; br < size; br += subSize)
            {
                for (int bc = 0; bc < size; bc += subSize)
                {
                    for (int num = 1; num <= size; num++)
                    {
                        int bit = 1 << (num - 1);
                        int rowFound = -1;
                        int colFound = -1;
                        int count = 0;
                        // Count positions in the box where candidate is possible.
                        for (int r = br; r < br + subSize; r++)
                        {
                            for (int c = bc; c < bc + subSize; c++)
                            {
                                if (board.Board[r, c] == 0)
                                {
                                    int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                                    if ((state.FullMask & ~used & bit) != 0)
                                    {
                                        count++;
                                        if (rowFound == -1)
                                            rowFound = r;
                                        else if (rowFound != r)
                                            rowFound = -2; // Not all in the same row.
                                        if (colFound == -1)
                                            colFound = c;
                                        else if (colFound != c)
                                            colFound = -2; // Not all in the same column.
                                    }
                                }
                            }
                        }
                        // If candidate appears only in one row within the box, eliminate it from that row outside the box.
                        if (count > 1 && rowFound >= 0)
                        {
                            for (int c = 0; c < size; c++)
                            {
                                if (c < bc || c >= bc + subSize)
                                {
                                    if (board.Board[rowFound, c] == 0)
                                    {
                                        int used = state.RowUsed[rowFound] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(rowFound, c)];
                                        int candidate = state.FullMask & ~used;
                                        if ((candidate & bit) != 0)
                                        {
                                            int newCandidate = candidate & ~bit;
                                            if (BitOperations.PopCount((uint)newCandidate) == 1)
                                            {
                                                int newBit = newCandidate;
                                                int val = BitOperations.TrailingZeroCount((uint)newBit) + 1;
                                                board.Board[rowFound, c] = val;
                                                state.RowUsed[rowFound] |= newBit;
                                                state.ColUsed[c] |= newBit;
                                                state.BoxUsed[state.GetBoxIndex(rowFound, c)] |= newBit;
                                                changed = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // If candidate appears only in one column within the box, eliminate it from that column outside the box.
                        if (count > 1 && colFound >= 0)
                        {
                            for (int r = 0; r < size; r++)
                            {
                                if (r < br || r >= br + subSize)
                                {
                                    if (board.Board[r, colFound] == 0)
                                    {
                                        int used = state.RowUsed[r] | state.ColUsed[colFound] | state.BoxUsed[state.GetBoxIndex(r, colFound)];
                                        int candidate = state.FullMask & ~used;
                                        if ((candidate & bit) != 0)
                                        {
                                            int newCandidate = candidate & ~bit;
                                            if (BitOperations.PopCount((uint)newCandidate) == 1)
                                            {
                                                int newBit = newCandidate;
                                                int val = BitOperations.TrailingZeroCount((uint)newBit) + 1;
                                                board.Board[r, colFound] = val;
                                                state.RowUsed[r] |= newBit;
                                                state.ColUsed[colFound] |= newBit;
                                                state.BoxUsed[state.GetBoxIndex(r, colFound)] |= newBit;
                                                changed = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return changed;
        }
    }
}
