using System;
using System.Collections.Generic;
using System.Numerics;

namespace omegaSudoku
{
    /// <summary>
    /// NakedPairsHeuristic with support for rows, columns, and boxes.
    /// If two empty cells in the same unit have exactly the same 2-candidate mask,
    /// remove those candidates from the other cells in that unit.
    /// </summary>
    public class NakedPairsHeuristic : IHeuristic
    {
        public bool Apply(SudokuBoard board, SolverState state)
        {
            bool changed = false;
            int size = state.Size;
            int subSize = state.SubSize;

            // 1. Rows
            for (int r = 0; r < size; r++)
            {
                if (ApplyNakedPairsInRow(board, state, r))
                    changed = true;
            }

            // 2. Columns
            for (int c = 0; c < size; c++)
            {
                if (ApplyNakedPairsInColumn(board, state, c))
                    changed = true;
            }

            // 3. Boxes
            for (int br = 0; br < size; br += subSize)
            {
                for (int bc = 0; bc < size; bc += subSize)
                {
                    if (ApplyNakedPairsInBox(board, state, br, bc))
                        changed = true;
                }
            }

            return changed;
        }

        private bool ApplyNakedPairsInRow(SudokuBoard board, SolverState state, int row)
        {
            bool changed = false;
            int size = state.Size;
            var pairCells = new Dictionary<int, List<int>>();

            // Find cells with exactly 2 candidates
            for (int c = 0; c < size; c++)
            {
                if (board.Board[row, c] == 0)
                {
                    int used = state.RowUsed[row] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(row, c)];
                    int candidate = state.FullMask & ~used;
                    if (BitOperations.PopCount((uint)candidate) == 2)
                    {
                        if (!pairCells.ContainsKey(candidate))
                            pairCells[candidate] = new List<int>();
                        pairCells[candidate].Add(c);
                    }
                }
            }
            // For each pair candidate that occurs exactly in 2 cells
            foreach (var kvp in pairCells)
            {
                if (kvp.Value.Count == 2)
                {
                    int pairMask = kvp.Key;
                    var columnsWithPair = kvp.Value;
                    // Remove pairMask from other cells in the row
                    for (int c = 0; c < size; c++)
                    {
                        if (!columnsWithPair.Contains(c) && board.Board[row, c] == 0)
                        {
                            int used = state.RowUsed[row] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(row, c)];
                            int candidate = state.FullMask & ~used;
                            int newCandidate = candidate & ~pairMask;
                            if (newCandidate != candidate && newCandidate != 0)
                            {
                                // If reduced to single candidate, place it
                                if (BitOperations.PopCount((uint)newCandidate) == 1)
                                {
                                    int bit = newCandidate;
                                    int val = BitOperations.TrailingZeroCount((uint)bit) + 1;
                                    board.Board[row, c] = val;
                                    state.RowUsed[row] |= bit;
                                    state.ColUsed[c] |= bit;
                                    state.BoxUsed[state.GetBoxIndex(row, c)] |= bit;
                                }
                                changed = true;
                            }
                        }
                    }
                }
            }

            return changed;
        }

        private bool ApplyNakedPairsInColumn(SudokuBoard board, SolverState state, int col)
        {
            bool changed = false;
            int size = state.Size;
            var pairCells = new Dictionary<int, List<int>>();

            for (int r = 0; r < size; r++)
            {
                if (board.Board[r, col] == 0)
                {
                    int used = state.RowUsed[r] | state.ColUsed[col] | state.BoxUsed[state.GetBoxIndex(r, col)];
                    int candidate = state.FullMask & ~used;
                    if (BitOperations.PopCount((uint)candidate) == 2)
                    {
                        if (!pairCells.ContainsKey(candidate))
                            pairCells[candidate] = new List<int>();
                        pairCells[candidate].Add(r);
                    }
                }
            }
            foreach (var kvp in pairCells)
            {
                if (kvp.Value.Count == 2)
                {
                    int pairMask = kvp.Key;
                    var rowsWithPair = kvp.Value;
                    for (int r = 0; r < size; r++)
                    {
                        if (!rowsWithPair.Contains(r) && board.Board[r, col] == 0)
                        {
                            int used = state.RowUsed[r] | state.ColUsed[col] | state.BoxUsed[state.GetBoxIndex(r, col)];
                            int candidate = state.FullMask & ~used;
                            int newCandidate = candidate & ~pairMask;
                            if (newCandidate != candidate && newCandidate != 0)
                            {
                                if (BitOperations.PopCount((uint)newCandidate) == 1)
                                {
                                    int bit = newCandidate;
                                    int val = BitOperations.TrailingZeroCount((uint)bit) + 1;
                                    board.Board[r, col] = val;
                                    state.RowUsed[r] |= bit;
                                    state.ColUsed[col] |= bit;
                                    state.BoxUsed[state.GetBoxIndex(r, col)] |= bit;
                                }
                                changed = true;
                            }
                        }
                    }
                }
            }
            return changed;
        }

        private bool ApplyNakedPairsInBox(SudokuBoard board, SolverState state, int boxRow, int boxCol)
        {
            bool changed = false;
            int size = state.Size;
            int subSize = state.SubSize;
            var pairCells = new Dictionary<int, List<(int, int)>>();

            // scan subgrid
            for (int r = boxRow; r < boxRow + subSize; r++)
            {
                for (int c = boxCol; c < boxCol + subSize; c++)
                {
                    if (board.Board[r, c] == 0)
                    {
                        int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                        int candidate = state.FullMask & ~used;
                        if (BitOperations.PopCount((uint)candidate) == 2)
                        {
                            if (!pairCells.ContainsKey(candidate))
                                pairCells[candidate] = new List<(int, int)>();
                            pairCells[candidate].Add((r, c));
                        }
                    }
                }
            }
            foreach (var kvp in pairCells)
            {
                if (kvp.Value.Count == 2)
                {
                    int pairMask = kvp.Key;
                    var cells = kvp.Value;
                    // Remove pairMask from other cells in the box
                    for (int r = boxRow; r < boxRow + subSize; r++)
                    {
                        for (int c = boxCol; c < boxCol + subSize; c++)
                        {
                            if (!cells.Contains((r, c)) && board.Board[r, c] == 0)
                            {
                                int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                                int candidate = state.FullMask & ~used;
                                int newCandidate = candidate & ~pairMask;
                                if (newCandidate != candidate && newCandidate != 0)
                                {
                                    if (BitOperations.PopCount((uint)newCandidate) == 1)
                                    {
                                        int bit = newCandidate;
                                        int val = BitOperations.TrailingZeroCount((uint)bit) + 1;
                                        board.Board[r, c] = val;
                                        state.RowUsed[r] |= bit;
                                        state.ColUsed[c] |= bit;
                                        state.BoxUsed[state.GetBoxIndex(r, c)] |= bit;
                                    }
                                    changed = true;
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
