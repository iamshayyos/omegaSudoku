using System;
using System.Collections.Generic;
using System.Numerics;

namespace omegaSudoku
{
    /// <summary>
    /// The main Sudoku solver class. For 9x9 and 16x16 boards, light heuristics are applied
    /// followed by standard backtracking. For 25x25 boards, advanced candidate management is used,
    /// starting with the subgrid (block) with the fewest candidates and selecting cells with high influence.
    /// </summary>
    public class SudokuSolver : ISudokuSolver
    {
        private readonly IHeuristic[] _heuristics;

        public SudokuSolver(IHeuristic[] heuristics)
        {
            _heuristics = heuristics;
        }

        public bool Solve(SudokuBoard board)
        {
            int size = board.Size;
            SolverState state = new SolverState(size);

            // Initialize the solver state (update row, column, and box bit masks)
            if (!InitializeState(board, state))
                return false;

            // For boards up to 16x16 (i.e., 9x9 and 16x16), apply light heuristics then use standard backtracking.
            if (size <= 16)
            {
                bool progress;
                do
                {
                    progress = false;
                    foreach (var heuristic in _heuristics)
                    {
                        if (heuristic.Apply(board, state))
                            progress = true;
                    }
                } while (progress);

                return BacktrackOptimized(board, state);
            }
            // For 25x25 boards, use advanced candidate management and subgrid selection.
            else if (size == 25)
            {
                // Step 1: Compute candidate bit masks for each empty cell.
                int[,] candidates = new int[size, size];
                for (int r = 0; r < size; r++)
                {
                    for (int c = 0; c < size; c++)
                    {
                        if (board.Board[r, c] == 0)
                        {
                            int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                            candidates[r, c] = state.FullMask & ~used;
                        }
                    }
                }

                // Step 2: Select the subgrid (block) with the lowest average candidate count.
                // For a 25x25 board, subgrid size is sqrt(25) = 5.
                int subSize = (int)Math.Sqrt(size); // For 25x25, subSize is 5.
                double bestAverage = double.MaxValue;
                int bestBr = 0, bestBc = 0; // Coordinates of the top-left cell of the best subgrid.
                for (int br = 0; br < size; br += subSize)
                {
                    for (int bc = 0; bc < size; bc += subSize)
                    {
                        int sumCandidateCount = 0;
                        int emptyCount = 0;
                        for (int r = br; r < br + subSize; r++)
                        {
                            for (int c = bc; c < bc + subSize; c++)
                            {
                                if (board.Board[r, c] == 0)
                                {
                                    int count = BitOperations.PopCount((uint)candidates[r, c]);
                                    sumCandidateCount += count;
                                    emptyCount++;
                                }
                            }
                        }
                        if (emptyCount > 0)
                        {
                            double avg = (double)sumCandidateCount / emptyCount;
                            if (avg < bestAverage)
                            {
                                bestAverage = avg;
                                bestBr = br;
                                bestBc = bc;
                            }
                        }
                    }
                }

                // Step 3: Within the chosen subgrid, build a list of empty cells with their candidate count and degree.
                List<CellInfo> subgridCells = new List<CellInfo>();
                for (int r = bestBr; r < bestBr + subSize; r++)
                {
                    for (int c = bestBc; c < bestBc + subSize; c++)
                    {
                        if (board.Board[r, c] == 0)
                        {
                            int cand = state.FullMask & ~(state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)]);
                            int count = BitOperations.PopCount((uint)cand);
                            int degree = CalculateDegree(board, state, r, c);
                            subgridCells.Add(new CellInfo(r, c, cand, count, degree));
                        }
                    }
                }
                // Sort cells: first by candidate count (ascending) and then by degree (descending).
                subgridCells.Sort((a, b) =>
                {
                    int cmp = a.CandidateCount.CompareTo(b.CandidateCount);
                    if (cmp == 0)
                        cmp = b.Degree.CompareTo(a.Degree);
                    return cmp;
                });

                // Step 4: Perform backtracking in the selected subgrid; once completed, continue with global backtracking.
                return BacktrackSubgrid(board, state, subgridCells);
            }

            return false;
        }

        /// <summary>
        /// Initializes the solver state by updating the row, column, and box bit masks based on the board.
        /// Returns false if a conflict is detected.
        /// </summary>
        private bool InitializeState(SudokuBoard board, SolverState state)
        {
            for (int r = 0; r < state.Size; r++)
            {
                for (int c = 0; c < state.Size; c++)
                {
                    int val = board.Board[r, c];
                    if (val != 0)
                    {
                        int bit = 1 << (val - 1);
                        int boxIndex = state.GetBoxIndex(r, c);
                        if ((state.RowUsed[r] & bit) != 0 ||
                            (state.ColUsed[c] & bit) != 0 ||
                            (state.BoxUsed[boxIndex] & bit) != 0)
                            return false; // Conflict detected
                        state.RowUsed[r] |= bit;
                        state.ColUsed[c] |= bit;
                        state.BoxUsed[boxIndex] |= bit;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Standard backtracking with MRV (Minimum Remaining Values) heuristic.
        /// Used for 9x9 and 16x16 boards.
        /// </summary>
        private bool BacktrackOptimized(SudokuBoard board, SolverState state)
        {
            int bestRow = -1, bestCol = -1;
            int bestCandidateCount = int.MaxValue;
            int bestCandidates = 0;

            // Find the empty cell with the minimum number of candidates (MRV).
            for (int r = 0; r < state.Size; r++)
            {
                for (int c = 0; c < state.Size; c++)
                {
                    if (board.Board[r, c] == 0)
                    {
                        int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                        int candidates = state.FullMask & ~used;
                        int count = BitOperations.PopCount((uint)candidates);
                        if (count == 0)
                            return false; // No candidates available, backtrack.
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
            }

            // If no empty cell is found, the board is solved.
            if (bestRow == -1)
                return true;

            // Try each candidate for the selected cell.
            while (bestCandidates != 0)
            {
                int bit = bestCandidates & -bestCandidates; // Get the lowest set bit.
                bestCandidates -= bit;
                int val = BitOperations.TrailingZeroCount((uint)bit) + 1;

                board.Board[bestRow, bestCol] = val;
                state.RowUsed[bestRow] |= bit;
                state.ColUsed[bestCol] |= bit;
                state.BoxUsed[state.GetBoxIndex(bestRow, bestCol)] |= bit;

                if (BacktrackOptimized(board, state))
                    return true;

                // Undo the assignment (backtracking).
                board.Board[bestRow, bestCol] = 0;
                state.RowUsed[bestRow] &= ~bit;
                state.ColUsed[bestCol] &= ~bit;
                state.BoxUsed[state.GetBoxIndex(bestRow, bestCol)] &= ~bit;
            }

            return false;
        }

        /// <summary>
        /// Backtracking within the selected subgrid (for 25x25 boards).
        /// It fills cells based on the sorted order (MRV and degree) within the subgrid and then continues globally.
        /// </summary>
        private bool BacktrackSubgrid(SudokuBoard board, SolverState state, List<CellInfo> subgridCells)
        {
            // If there are no more cells in the subgrid, continue with global backtracking.
            if (subgridCells.Count == 0)
                return BacktrackOptimized(board, state);

            // Select the first cell from the sorted list.
            CellInfo cell = subgridCells[0];
            subgridCells.RemoveAt(0);
            int r = cell.Row, c = cell.Col;

            // Compute the current candidate mask for this cell.
            int candidateMask = state.FullMask & ~(state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)]);

            while (candidateMask != 0)
            {
                int bit = candidateMask & -candidateMask;
                candidateMask -= bit;
                int val = BitOperations.TrailingZeroCount((uint)bit) + 1;

                board.Board[r, c] = val;
                state.RowUsed[r] |= bit;
                state.ColUsed[c] |= bit;
                state.BoxUsed[state.GetBoxIndex(r, c)] |= bit;

                if (BacktrackSubgrid(board, state, subgridCells))
                    return true;

                // Undo the assignment if it leads to a dead-end.
                board.Board[r, c] = 0;
                state.RowUsed[r] &= ~bit;
                state.ColUsed[c] &= ~bit;
                state.BoxUsed[state.GetBoxIndex(r, c)] &= ~bit;
            }

            // Return the cell back to the list if no valid assignment was found.
            subgridCells.Insert(0, cell);
            return false;
        }

        /// <summary>
        /// Calculates the "degree" of a cell, i.e. the number of empty neighbor cells in the same row,
        /// column, and subgrid. This value indicates how influential the cell is.
        /// </summary>
        private int CalculateDegree(SudokuBoard board, SolverState state, int r, int c)
        {
            int size = board.Size;
            HashSet<(int, int)> neighbors = new HashSet<(int, int)>();

            // Neighbors in the same row.
            for (int j = 0; j < size; j++)
            {
                if (j != c && board.Board[r, j] == 0)
                    neighbors.Add((r, j));
            }
            // Neighbors in the same column.
            for (int i = 0; i < size; i++)
            {
                if (i != r && board.Board[i, c] == 0)
                    neighbors.Add((i, c));
            }
            // Neighbors in the same subgrid (block).
            int subSize = state.SubSize;
            int startRow = (r / subSize) * subSize;
            int startCol = (c / subSize) * subSize;
            for (int i = startRow; i < startRow + subSize; i++)
            {
                for (int j = startCol; j < startCol + subSize; j++)
                {
                    if ((i != r || j != c) && board.Board[i, j] == 0)
                        neighbors.Add((i, j));
                }
            }

            return neighbors.Count;
        }
    }

    /// <summary>
    /// Helper class to store information about an empty cell:
    /// its position, candidate bit mask, number of candidates, and its degree (influence).
    /// </summary>
    internal class CellInfo
    {
        public int Row { get; }
        public int Col { get; }
        public int CandidateMask { get; }
        public int CandidateCount { get; }
        public int Degree { get; }

        public CellInfo(int row, int col, int candidateMask, int candidateCount, int degree)
        {
            Row = row;
            Col = col;
            CandidateMask = candidateMask;
            CandidateCount = candidateCount;
            Degree = degree;
        }
    }
}
