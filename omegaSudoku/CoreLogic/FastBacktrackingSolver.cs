using System;
using System.Collections.Generic;
using System.Numerics;
using omegaSudoku.Interfaces;
using omegaSudoku.Board;
using omegaSudoku.Exceptions;

namespace omegaSudoku.CoreLogic
{
    /// <summary>
    /// Backtracking solver using bit-level operations and the MRV heuristic.
    /// This solver precomputes the list of empty cells and calculates candidate masks during the solving.
    /// </summary>
    public class FastBacktrackingSolver : ISudokuSolver
    {
        // Board parameters
        private int size;
        private int subSize;
        private int fullMask;

        // Constraint masks for rows, columns and boxes
        private int[] rowUsed;
        private int[] colUsed;
        private int[] boxUsed;

        // Reference to the board array and list of empty cells
        private int[,] board;
        private List<(int r, int c)> empties;

        /// <summary>
        /// Solve the given SudokuBoard.
        /// </summary>
        /// <param name="sudokuBoard">The Sudoku board to solve.</param>
        /// <returns>True if solved; otherwise, an UnsolvableBoardException is thrown.</returns>
        public bool Solve(SudokuBoard sudokuBoard)
        {
            size = sudokuBoard.Size;
            subSize = (int)Math.Sqrt(size);
            fullMask = (1 << size) - 1; // For a 9x9 board: (1 << 9) - 1 = 511

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

            // Begin recursion starting at position 0 in the empties list.
            if (SolveRecursively(0))
            {
                return true;
            }
            else
            {
                // Instead of returning false, throw the exception
                throw new UnsolvableBoardException("Board cannot be solved.");
            }
        }

        /// <summary>
        /// Recursively solve the puzzle by assigning values to empty cells.
        /// Uses the MRV heuristic: among cells from current position onward, selects the one with the fewest candidates.
        /// </summary>
        /// <param name="pos">The current index in the empties list to process.</param>
        /// <returns>True if a complete solution is found, false if backtracking is required.</returns>
        private bool SolveRecursively(int pos)
        {
            // If we've assigned all empty cells, puzzle solved.
            if (pos == empties.Count)
                return true;

            // MRV: find the cell with the minimum candidate count among empties[pos...end]
            int minCandidates = int.MaxValue;
            int selectedIndex = pos;
            int candidateMask = 0;
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

            // If no candidates available for the selected cell, backtrack.
            if (minCandidates == 0)
                return false;

            // Swap the selected cell into the current position (fixing order permanently)
            (int r, int c) temp = empties[pos];
            empties[pos] = empties[selectedIndex];
            empties[selectedIndex] = temp;

            // Get coordinates and box index of the current cell.
            (int r0, int c0) = empties[pos];
            int boxIdx = (r0 / subSize) * subSize + (c0 / subSize);

            // Try every candidate (bit) for the cell.
            while (candidateMask != 0)
            {
                int bit = candidateMask & -candidateMask; // extract lowest set bit
                candidateMask -= bit;
                int val = BitOperations.TrailingZeroCount((uint)bit) + 1;

                // Place the candidate value.
                board[r0, c0] = val;
                rowUsed[r0] |= bit;
                colUsed[c0] |= bit;
                boxUsed[boxIdx] |= bit;

                // Recurse to assign the next cell.
                if (SolveRecursively(pos + 1))
                    return true;

                // Backtrack: undo the assignment.
                board[r0, c0] = 0;
                rowUsed[r0] &= ~bit;
                colUsed[c0] &= ~bit;
                boxUsed[boxIdx] &= ~bit;
            }

            // No candidate led to a solution – backtrack.
            return false;
        }
    }
}
