using System;
using System.Collections.Generic;
using System.Numerics;
using omegaSudoku.Interfaces;
using omegaSudoku.Board;

namespace omegaSudoku.CoreLogic
{
    /// <summary>
    /// Solver for big boards.
    /// Uses heavy constraint propagation (naked singles), MRV heuristic with swapping,
    /// and periodic global constraint checks (every CHECK_INTERVAL levels) to prune unsolvable branches fast.
    /// Optimized to solve these boards quickly without degrading performance for smaller boards.
    /// </summary>
    public class FastAdvanced25x25Solver : ISudokuSolver
    {
        private int size;
        private int subSize;
        private int fullMask; // For 25x25: (1 << 25) - 1

        private int[,] board;
        private int[] rowUsed;
        private int[] colUsed;
        private int[] boxUsed;

        // List of empty cell coordinates (row, column)
        private List<(int r, int c)> empties;

        // Perform global constraint check every CHECK_INTERVAL recursion levels.
        private const int CHECK_INTERVAL = 10;

        /// <summary>
        /// Solve the given Sudoku board.
        /// </summary>
        /// <param name="sudokuBoard">The board to solve.</param>
        /// <returns>True if solved, false otherwise.</returns>
        public bool Solve(SudokuBoard sudokuBoard)
        {
            size = sudokuBoard.Size;
            subSize = (int)Math.Sqrt(size);
            fullMask = (1 << size) - 1;

            board = sudokuBoard.Board;
            rowUsed = new int[size];
            colUsed = new int[size];
            boxUsed = new int[size];
            empties = new List<(int, int)>();

            // Initialize constraint masks and collect empty cells.
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    int val = board[r, c];
                    if (val == 0)
                        empties.Add((r, c));
                    else
                    {
                        int bit = 1 << (val - 1);
                        rowUsed[r] |= bit;
                        colUsed[c] |= bit;
                        int boxIndex = (r / subSize) * subSize + (c / subSize);
                        boxUsed[boxIndex] |= bit;
                    }
                }
            }

            // For boards 16x16 and larger, apply naked singles propagation to prune domains.
            if (size >= 16)
            {
                if (!ApplyNakedSingles())
                    return false;
                RebuildEmpties();
            }

            return SolveRecursively(0);
        }

        /// <summary>
        /// Applies naked singles propagation repeatedly.
        /// If any cell has zero candidates, returns false immediately.
        /// </summary>
        private bool ApplyNakedSingles()
        {
            bool progress;
            do
            {
                progress = false;
                for (int r = 0; r < size; r++)
                {
                    for (int c = 0; c < size; c++)
                    {
                        if (board[r, c] != 0)
                            continue;
                        int boxIdx = (r / subSize) * subSize + (c / subSize);
                        int candidates = fullMask & ~(rowUsed[r] | colUsed[c] | boxUsed[boxIdx]);
                        int count = BitOperations.PopCount((uint)candidates);
                        if (count == 0)
                            return false; // Contradiction found.
                        if (count == 1)
                        {
                            int bit = candidates & -candidates; // lowest set bit
                            int val = BitOperations.TrailingZeroCount((uint)bit) + 1;
                            board[r, c] = val;
                            rowUsed[r] |= bit;
                            colUsed[c] |= bit;
                            boxUsed[boxIdx] |= bit;
                            progress = true;
                        }
                    }
                }
            } while (progress);
            return true;
        }

        /// <summary>
        /// Rebuilds the empties list to include only cells that remain unassigned.
        /// </summary>
        private void RebuildEmpties()
        {
            var newEmpties = new List<(int, int)>();
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (board[r, c] == 0)
                        newEmpties.Add((r, c));
                }
            }
            empties = newEmpties;
        }

        /// <summary>
        /// Global constraint check: for each row, column, and box,
        /// for every missing digit, ensure there is at least one empty cell
        /// whose candidate mask allows that digit.
        /// Returns false if any unit is unsolvable.
        /// </summary>
        private bool GlobalConstraintCheck()
        {
            // Check rows.
            for (int r = 0; r < size; r++)
            {
                int missing = fullMask & ~rowUsed[r];
                if (missing != 0)
                {
                    for (int digit = 1; digit <= size; digit++)
                    {
                        int bit = 1 << (digit - 1);
                        if ((missing & bit) != 0)
                        {
                            bool found = false;
                            for (int c = 0; c < size; c++)
                            {
                                if (board[r, c] == 0)
                                {
                                    int boxIndex = (r / subSize) * subSize + (c / subSize);
                                    int candidates = fullMask & ~(rowUsed[r] | colUsed[c] | boxUsed[boxIndex]);
                                    if ((candidates & bit) != 0)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                            }
                            if (!found)
                                return false;
                        }
                    }
                }
            }

            // Check columns.
            for (int c = 0; c < size; c++)
            {
                int missing = fullMask & ~colUsed[c];
                if (missing != 0)
                {
                    for (int digit = 1; digit <= size; digit++)
                    {
                        int bit = 1 << (digit - 1);
                        if ((missing & bit) != 0)
                        {
                            bool found = false;
                            for (int r = 0; r < size; r++)
                            {
                                if (board[r, c] == 0)
                                {
                                    int boxIndex = (r / subSize) * subSize + (c / subSize);
                                    int candidates = fullMask & ~(rowUsed[r] | colUsed[c] | boxUsed[boxIndex]);
                                    if ((candidates & bit) != 0)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                            }
                            if (!found)
                                return false;
                        }
                    }
                }
            }

            // Check boxes.
            for (int br = 0; br < subSize; br++)
            {
                for (int bc = 0; bc < subSize; bc++)
                {
                    int boxIndex = br * subSize + bc;
                    int missing = fullMask & ~boxUsed[boxIndex];
                    if (missing != 0)
                    {
                        for (int digit = 1; digit <= size; digit++)
                        {
                            int bit = 1 << (digit - 1);
                            if ((missing & bit) != 0)
                            {
                                bool found = false;
                                for (int r = br * subSize; r < br * subSize + subSize; r++)
                                {
                                    for (int c = bc * subSize; c < bc * subSize + subSize; c++)
                                    {
                                        if (board[r, c] == 0)
                                        {
                                            int candidates = fullMask & ~(rowUsed[r] | colUsed[c] | boxUsed[boxIndex]);
                                            if ((candidates & bit) != 0)
                                            {
                                                found = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (found)
                                        break;
                                }
                                if (!found)
                                    return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Recursively assigns values to empty cells using MRV selection.
        /// Periodically performs a global constraint check (every CHECK_INTERVAL levels) to fail fast on unsolvable branches.
        /// </summary>
        /// <param name="pos">Current index in the empties list.</param>
        /// <returns>True if a complete solution is found, false if backtracking is required.</returns>
        private bool SolveRecursively(int pos)
        {
            if (pos % CHECK_INTERVAL == 0)
            {
                if (!GlobalConstraintCheck())
                    return false;
            }

            if (pos == empties.Count)
                return true; // All cells assigned.

            int minCandidates = int.MaxValue;
            int selectedIndex = pos;
            int candidateMask = 0;
            // MRV: select the cell with the fewest candidates.
            for (int i = pos; i < empties.Count; i++)
            {
                (int r, int c) = empties[i];
                int boxIndex = (r / subSize) * subSize + (c / subSize);
                int candidates = fullMask & ~(rowUsed[r] | colUsed[c] | boxUsed[boxIndex]);
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
            if (minCandidates == 0)
                return false;

            // Swap the selected cell into the current position.
            (int r, int c) temp = empties[pos];
            empties[pos] = empties[selectedIndex];
            empties[selectedIndex] = temp;

            (int r0, int c0) = empties[pos];
            int boxIdx = (r0 / subSize) * subSize + (c0 / subSize);

            // Try every candidate (each set bit) for the selected cell.
            while (candidateMask != 0)
            {
                int bit = candidateMask & -candidateMask; // Extract lowest set bit.
                candidateMask -= bit;
                int val = BitOperations.TrailingZeroCount((uint)bit) + 1;

                board[r0, c0] = val;
                rowUsed[r0] |= bit;
                colUsed[c0] |= bit;
                boxUsed[boxIdx] |= bit;

                if (SolveRecursively(pos + 1))
                    return true;

                // Backtrack: undo the assignment.
                board[r0, c0] = 0;
                rowUsed[r0] &= ~bit;
                colUsed[c0] &= ~bit;
                boxUsed[boxIdx] &= ~bit;
            }

            return false;
        }
    }
}
