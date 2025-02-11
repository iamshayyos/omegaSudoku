using omegaSudoku.Board;
using omegaSudoku.CoreLogic;
using omegaSudoku.Interfaces;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace omegaSudoku.Heuristics
{
    /// <summary>
    /// Hidden Pairs Heuristic:
    /// For each row/column/box, if there are exactly two cells
    /// where a certain pair of digits can appear (and no other digits can appear in those two cells),
    /// we can eliminate other candidates from those two cells.
    /// </summary>
    public class HiddenPairsHeuristic : IHeuristic
    {
        public bool Apply(SudokuBoard board, SolverState state)
        {
            bool changed = false;
            int size = state.Size;

            // Apply hidden pairs on rows
            for (int r = 0; r < size; r++)
            {
                if (ApplyHiddenPairsInUnit(board, state, r, true))
                    changed = true;
            }

            // Apply hidden pairs on columns
            for (int c = 0; c < size; c++)
            {
                if (ApplyHiddenPairsInUnit(board, state, c, false))
                    changed = true;
            }

            // Apply hidden pairs on boxes
            int subSize = state.SubSize;
            for (int boxRow = 0; boxRow < size; boxRow += subSize)
            {
                for (int boxCol = 0; boxCol < size; boxCol += subSize)
                {
                    if (ApplyHiddenPairsInBox(board, state, boxRow, boxCol))
                        changed = true;
                }
            }

            return changed;
        }

        /// <summary>
        /// Apply hidden pairs to a single unit (row or column).
        /// If isRow == true, 'index' is row index; otherwise 'index' is column index.
        /// </summary>
        private bool ApplyHiddenPairsInUnit(SudokuBoard board, SolverState state, int index, bool isRow)
        {
            bool changed = false;
            int size = state.Size;

            // candidatePositions[digit] = list of positions where 'digit' can appear
            var candidatePositions = new Dictionary<int, List<int>>();
            for (int digit = 1; digit <= size; digit++)
            {
                candidatePositions[digit] = new List<int>();
            }

            // Collect possible positions of each digit in this row/col
            for (int i = 0; i < size; i++)
            {
                int r = isRow ? index : i;
                int c = isRow ? i : index;

                if (board.Board[r, c] == 0)
                {
                    int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                    int mask = state.FullMask & ~used;
                    // For each digit that can appear
                    for (int digit = 1; digit <= size; digit++)
                    {
                        int bit = 1 << digit - 1;
                        if ((mask & bit) != 0)
                        {
                            candidatePositions[digit].Add(i); // store i as the position in the row/col
                        }
                    }
                }
            }

            // Now find pairs of digits that share exactly the same 2 positions
            var pairs = new Dictionary<(int, int), List<int>>();
            // Key: (pos1, pos2), Value: list of digits that appear exactly in those 2 positions

            for (int digit = 1; digit <= size; digit++)
            {
                if (candidatePositions[digit].Count == 2)
                {
                    var posList = candidatePositions[digit];
                    int p1 = posList[0];
                    int p2 = posList[1];
                    var key = (Math.Min(p1, p2), Math.Max(p1, p2));
                    if (!pairs.ContainsKey(key))
                        pairs[key] = new List<int>();
                    pairs[key].Add(digit);
                }
            }

            // For each pair of positions that hold exactly 2 digits, remove other candidates
            foreach (var kvp in pairs)
            {
                var positions = kvp.Key; // (pos1, pos2)
                var digits = kvp.Value;
                if (digits.Count == 2)
                {
                    // We have a hidden pair
                    // Let’s remove other candidates from these two cells
                    int digitMask = 0;
                    foreach (var d in digits)
                    {
                        digitMask |= 1 << d - 1;
                    }
                    int[] arrPos = new int[] { positions.Item1, positions.Item2 };

                    foreach (var pos in arrPos)
                    {
                        int r = isRow ? index : pos;
                        int c = isRow ? pos : index;
                        if (board.Board[r, c] == 0)
                        {
                            int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                            int currentMask = state.FullMask & ~used;
                            int newMask = currentMask & digitMask; // keep only the 2 digits
                            if (newMask != currentMask)
                            {
                                // If the newMask has exactly 1 bit, we can place the digit
                                if (BitOperations.PopCount((uint)newMask) == 1)
                                {
                                    int bit = newMask;
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

            return changed;
        }

        /// <summary>
        /// Apply hidden pairs in a subgrid (box) with top-left corner (boxRow, boxCol).
        /// </summary>
        private bool ApplyHiddenPairsInBox(SudokuBoard board, SolverState state, int boxRow, int boxCol)
        {
            bool changed = false;
            int size = state.Size;
            int subSize = state.SubSize;

            // digit -> list of positions (row,col) in the box
            var candidatePositions = new Dictionary<int, List<(int, int)>>();
            for (int d = 1; d <= size; d++)
            {
                candidatePositions[d] = new List<(int, int)>();
            }

            // Collect possible positions of each digit in this box
            for (int r = boxRow; r < boxRow + subSize; r++)
            {
                for (int c = boxCol; c < boxCol + subSize; c++)
                {
                    if (board.Board[r, c] == 0)
                    {
                        int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                        int mask = state.FullMask & ~used;
                        for (int d = 1; d <= size; d++)
                        {
                            int bit = 1 << d - 1;
                            if ((mask & bit) != 0)
                            {
                                candidatePositions[d].Add((r, c));
                            }
                        }
                    }
                }
            }

            // Find pairs of digits that share exactly the same 2 cells
            var pairs = new Dictionary<((int, int), (int, int)), List<int>>();
            foreach (var d in candidatePositions.Keys)
            {
                if (candidatePositions[d].Count == 2)
                {
                    var listPos = candidatePositions[d];
                    var p1 = listPos[0];
                    var p2 = listPos[1];
                    var key = OrderTuple(p1, p2);
                    if (!pairs.ContainsKey(key))
                        pairs[key] = new List<int>();
                    pairs[key].Add(d);
                }
            }

            // For each pair of positions that hold exactly 2 digits, remove other candidates from those cells
            foreach (var kvp in pairs)
            {
                var posPair = kvp.Key;
                var digits = kvp.Value;
                if (digits.Count == 2)
                {
                    // hidden pair
                    int digitMask = 0;
                    foreach (var d in digits)
                        digitMask |= 1 << d - 1;

                    var cells = new List<(int, int)> { posPair.Item1, posPair.Item2 };
                    foreach (var (r, c) in cells)
                    {
                        int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                        int currMask = state.FullMask & ~used;
                        int newMask = currMask & digitMask;
                        if (newMask != currMask)
                        {
                            // If the newMask has exactly 1 bit, we can place the digit
                            if (BitOperations.PopCount((uint)newMask) == 1)
                            {
                                int bit = newMask;
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

            return changed;
        }

        /// <summary>
        /// Helper to ensure the tuple is stored in a consistent order (lowest row/col first).
        /// </summary>
        private ((int, int), (int, int)) OrderTuple((int, int) p1, (int, int) p2)
        {
            if (p1.Item1 < p2.Item1) return (p1, p2);
            if (p1.Item1 > p2.Item1) return (p2, p1);
            // same row
            if (p1.Item2 < p2.Item2) return (p1, p2);
            return (p2, p1);
        }
    }
}
