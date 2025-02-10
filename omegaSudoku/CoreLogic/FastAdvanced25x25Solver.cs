using System;
using System.Collections.Generic;
using System.Numerics;
using omegaSudoku.Interfaces;
using omegaSudoku.BoardAndCells;

namespace omegaSudoku.CoreLogic
{
    /// <summary>
    /// solver for 25x25 Sudoku puzzles.
    /// Uses heavy constraint propagation (naked singles), a precomputed empty cells list with MRV heuristic (with swapping),
    /// and bit-level operations for candidate computation and in-place constraint updates.
    /// </summary>
    public class FastAdvanced25x25Solver : ISudokuSolver
    {
        private int size;
        private int subSize;
        private int fullMask; // For 25x25: fullMask = (1 << 25) - 1

        private int[,] board;
        private int[] rowUsed;
        private int[] colUsed;
        private int[] boxUsed;

        // List of empty cell coordinates (row, column)
        private List<(int r, int c)> empties;

        /// <summary>
        /// Main method to solve the given Sudoku board.
        /// </summary>
        /// <param name="sudokuBoard">The Sudoku board to solve.</param>
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
                    {
                        empties.Add((r, c));
                    }
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

            //Apply Naked Singles 
            if (!ApplyNakedSingles())
                return false;
            // Rebuild empties list after propagation.
            RebuildEmpties();

            // Start the recursive backtracking.
            return SolveRecursively(0);
        }

        /// <summary>
        /// Applies the "naked singles" technique: if an empty cell has exactly one candidate, fill it immediately.
        /// Repeats until no progress is made.
        /// </summary>
        /// <returns>True if no contradiction is found, false otherwise.</returns>
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
                            int bit = candidates & -candidates; // Lowest set bit.
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
        /// Rebuilds the empties list to contain only the cells that are still empty.
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
        /// Recursively assigns values to empty cells using the MRV heuristic.
        /// Uses swapping in the empties list to fix the order and avoid re-scanning the entire list.
        /// </summary>
        /// <param name="pos">Current index in the empties list.</param>
        /// <returns>True if a solution is found, false otherwise.</returns>
        private bool SolveRecursively(int pos)
        {
            if (pos == empties.Count)
                return true; // All cells assigned.

            int minCandidates = int.MaxValue;
            int selectedIndex = pos;
            int candidateMask = 0;
            // MRV: select the cell with the fewest candidates among empties
            for (int i = pos; i < empties.Count; i++)
            {
                (int r, int c) = empties[i];
                int boxIdx = (r / subSize) * subSize + (c / subSize);
                int candidates = fullMask & ~(rowUsed[r] | colUsed[c] | boxUsed[boxIdx]);
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
                return false; // Dead end.

            // Swap the selected cell into the current position.
            (int r, int c) temp = empties[pos];
            empties[pos] = empties[selectedIndex];
            empties[selectedIndex] = temp;

            (int r0, int c0) = empties[pos];
            int boxIndex = (r0 / subSize) * subSize + (c0 / subSize);

            // Try each candidate for the selected cell.
            while (candidateMask != 0)
            {
                int bit = candidateMask & -candidateMask; // Extract lowest set bit.
                candidateMask -= bit;
                int val = BitOperations.TrailingZeroCount((uint)bit) + 1;

                // Place the candidate.
                board[r0, c0] = val;
                rowUsed[r0] |= bit;
                colUsed[c0] |= bit;
                boxUsed[boxIndex] |= bit;

                if (SolveRecursively(pos + 1))
                    return true;

                //  undo the assignment.
                board[r0, c0] = 0;
                rowUsed[r0] &= ~bit;
                colUsed[c0] &= ~bit;
                boxUsed[boxIndex] &= ~bit;
            }

            return false;
        }
    }
}
