using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace omegaSudoku
{
    /// <summary>
    /// The main Sudoku solver class.
    /// Includes:
    /// - Light heuristics for 9x9 and 16x16 (SingleCandidate, HiddenSingle, NakedPairs, HiddenPairs, etc.).
    /// - Advanced subgrid selection for 25x25 with MRV & degree heuristics.
    /// - Forward checking with fail-fast (zero-candidate check) & missing-number check in each row/col/box.
    /// - Transposition table to avoid repeated states.
    /// </summary>
    public class SudokuSolver : ISudokuSolver
    {
        private readonly IHeuristic[] _heuristics;

        // Cache of unsolvable states
        private static HashSet<string> transpositionTable = new HashSet<string>();

        public SudokuSolver(IHeuristic[] heuristics)
        {
            _heuristics = heuristics;
        }

        public bool Solve(SudokuBoard board)
        {
            int size = board.Size;
            SolverState state = new SolverState(size);

            // 1. Initialize the solver state (row, column, and box bit masks)
            if (!InitializeState(board, state))
                return false;

            // 2. For smaller boards (9x9, 16x16): apply heuristics, then standard backtracking
            if (size <= 16)
            {
                bool progress;
                do
                {
                    progress = false;
                    foreach (var heuristic in _heuristics)
                    {
                        if (heuristic.Apply(board, state))
                        {
                            // After each heuristic application, run ForwardCheck
                            if (!ForwardCheck(board, state))
                                return false; // If forward check fails, no solution
                            progress = true;
                        }
                    }
                } while (progress);

                return BacktrackOptimized(board, state);
            }
            // 3. For 25x25 boards: advanced subgrid selection + backtracking
            else if (size == 25)
            {
                // Compute candidates for each empty cell
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

                // Select subgrid (block) with lowest average candidate count
                int subSize = (int)Math.Sqrt(size);
                double bestAverage = double.MaxValue;
                int bestBr = 0, bestBc = 0;
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
                            // Optionally add more subgrid selection logic here
                            if (avg < bestAverage)
                            {
                                bestAverage = avg;
                                bestBr = br;
                                bestBc = bc;
                            }
                        }
                    }
                }

                // Build subgrid cell list
                List<CellInfo> subgridCells = new List<CellInfo>();
                for (int r = bestBr; r < bestBr + subSize; r++)
                {
                    for (int c = bestBc; c < bestBc + subSize; c++)
                    {
                        if (board.Board[r, c] == 0)
                        {
                            int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                            int cand = state.FullMask & ~used;
                            int count = BitOperations.PopCount((uint)cand);
                            int degree = CalculateDegree(board, state, r, c);
                            subgridCells.Add(new CellInfo(r, c, cand, count, degree));
                        }
                    }
                }
                // Sort by candidateCount (asc), then by degree (desc)
                subgridCells.Sort((a, b) =>
                {
                    int cmp = a.CandidateCount.CompareTo(b.CandidateCount);
                    if (cmp == 0)
                        cmp = b.Degree.CompareTo(a.Degree);
                    return cmp;
                });

                return BacktrackSubgrid(board, state, subgridCells);
            }

            return false;
        }

        /// <summary>
        /// Initializes row, col, and box masks according to current board.
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
                            return false;
                        state.RowUsed[r] |= bit;
                        state.ColUsed[c] |= bit;
                        state.BoxUsed[boxIndex] |= bit;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Standard backtracking with MRV, plus forward checking and transposition table.
        /// </summary>
        private bool BacktrackOptimized(SudokuBoard board, SolverState state)
        {
            // Check if we've seen this configuration
            string hash = GetBoardHash(board);
            if (transpositionTable.Contains(hash))
                return false;

            int bestRow = -1, bestCol = -1;
            int bestCandidateCount = int.MaxValue;
            int bestCandidates = 0;

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
                            return false; // fail-fast: no candidates

                        if (count < bestCandidateCount)
                        {
                            bestCandidateCount = count;
                            bestCandidates = candidates;
                            bestRow = r;
                            bestCol = c;
                            if (count == 1) // immediate pick
                                break;
                        }
                    }
                }
            }

            // No empty cells -> solved
            if (bestRow == -1)
                return true;

            // Try each candidate
            while (bestCandidates != 0)
            {
                int bit = bestCandidates & -bestCandidates;
                bestCandidates -= bit;
                int val = BitOperations.TrailingZeroCount((uint)bit) + 1;

                // Assign
                board.Board[bestRow, bestCol] = val;
                state.RowUsed[bestRow] |= bit;
                state.ColUsed[bestCol] |= bit;
                state.BoxUsed[state.GetBoxIndex(bestRow, bestCol)] |= bit;

                // Forward check
                if (ForwardCheck(board, state) && BacktrackOptimized(board, state))
                    return true;

                // Undo assignment
                board.Board[bestRow, bestCol] = 0;
                state.RowUsed[bestRow] &= ~bit;
                state.ColUsed[bestCol] &= ~bit;
                state.BoxUsed[state.GetBoxIndex(bestRow, bestCol)] &= ~bit;
            }

            // Mark as unsolvable in this path
            transpositionTable.Add(hash);
            return false;
        }

        /// <summary>
        /// Backtracking for the selected subgrid in 25x25 boards, continuing with global backtracking afterwards.
        /// </summary>
        private bool BacktrackSubgrid(SudokuBoard board, SolverState state, List<CellInfo> subgridCells)
        {
            string hash = GetBoardHash(board);
            if (transpositionTable.Contains(hash))
                return false;

            if (subgridCells.Count == 0)
                return BacktrackOptimized(board, state);

            var cell = subgridCells[0];
            subgridCells.RemoveAt(0);

            int r = cell.Row, c = cell.Col;
            int candidateMask = state.FullMask & ~(state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)]);

            while (candidateMask != 0)
            {
                int bit = candidateMask & -candidateMask;
                candidateMask -= bit;
                int val = BitOperations.TrailingZeroCount((uint)bit) + 1;

                // Assign
                board.Board[r, c] = val;
                state.RowUsed[r] |= bit;
                state.ColUsed[c] |= bit;
                state.BoxUsed[state.GetBoxIndex(r, c)] |= bit;

                if (ForwardCheck(board, state) && BacktrackSubgrid(board, state, subgridCells))
                    return true;

                // Undo
                board.Board[r, c] = 0;
                state.RowUsed[r] &= ~bit;
                state.ColUsed[c] &= ~bit;
                state.BoxUsed[state.GetBoxIndex(r, c)] &= ~bit;
            }

            subgridCells.Insert(0, cell);
            transpositionTable.Add(hash);
            return false;
        }

        /// <summary>
        /// ForwardCheck: verify no empty cell has zero candidates,
        /// and each row, column, and box can still place all required digits (Missing Number Check).
        /// </summary>
        private bool ForwardCheck(SudokuBoard board, SolverState state)
        {
            int size = state.Size;
            // 1) Zero-candidate check
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (board.Board[r, c] == 0)
                    {
                        int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                        int candidates = state.FullMask & ~used;
                        if (candidates == 0)
                            return false; // fail-fast
                    }
                }
            }

            // 2) Missing Number Check (for each row, col, box)
            if (!CheckRowColBoxMissing(board, state))
                return false;

            return true;
        }

        /// <summary>
        /// Ensures that for each row, column, and box, every digit (1..Size)
        /// has at least one possible cell to go into.
        /// If any digit can't appear anywhere, fail fast.
        /// </summary>
        private bool CheckRowColBoxMissing(SudokuBoard board, SolverState state)
        {
            int size = state.Size;
            int full = state.FullMask;

            // For each row
            for (int r = 0; r < size; r++)
            {
                // "used" mask includes the digits already placed in that row
                int rowUsed = state.RowUsed[r];
                // If a digit isn't placed, it must have at least one cell candidate
                // we'll check which columns are free
                for (int digit = 1; digit <= size; digit++)
                {
                    int bit = 1 << (digit - 1);
                    // If digit is not used yet in row, check if there's any cell for it
                    if ((rowUsed & bit) == 0)
                    {
                        bool canPlace = false;
                        for (int c = 0; c < size && !canPlace; c++)
                        {
                            if (board.Board[r, c] == 0)
                            {
                                int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                                if ((used & bit) == 0)
                                    canPlace = true;
                            }
                        }
                        if (!canPlace) return false;
                    }
                }
            }

            // For each column
            for (int c = 0; c < size; c++)
            {
                int colUsed = state.ColUsed[c];
                for (int digit = 1; digit <= size; digit++)
                {
                    int bit = 1 << (digit - 1);
                    if ((colUsed & bit) == 0)
                    {
                        bool canPlace = false;
                        for (int r = 0; r < size && !canPlace; r++)
                        {
                            if (board.Board[r, c] == 0)
                            {
                                int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                                if ((used & bit) == 0)
                                    canPlace = true;
                            }
                        }
                        if (!canPlace) return false;
                    }
                }
            }

            // For each box
            int subSize = state.SubSize;
            for (int boxRow = 0; boxRow < size; boxRow += subSize)
            {
                for (int boxCol = 0; boxCol < size; boxCol += subSize)
                {
                    // Collect used for this box
                    int boxUsed = 0;
                    for (int r = boxRow; r < boxRow + subSize; r++)
                    {
                        for (int c = boxCol; c < boxCol + subSize; c++)
                        {
                            int val = board.Board[r, c];
                            if (val != 0)
                                boxUsed |= (1 << (val - 1));
                        }
                    }
                    for (int digit = 1; digit <= size; digit++)
                    {
                        int bit = 1 << (digit - 1);
                        if ((boxUsed & bit) == 0)
                        {
                            // digit not placed in this box, check if there's any cell for it
                            bool canPlace = false;
                            for (int r = boxRow; r < boxRow + subSize && !canPlace; r++)
                            {
                                for (int c = boxCol; c < boxCol + subSize && !canPlace; c++)
                                {
                                    if (board.Board[r, c] == 0)
                                    {
                                        int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                                        if ((used & bit) == 0)
                                            canPlace = true;
                                    }
                                }
                            }
                            if (!canPlace) return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a string representing the current board state for transposition table.
        /// </summary>
        private string GetBoardHash(SudokuBoard board)
        {
            StringBuilder sb = new StringBuilder();
            int size = board.Size;
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    sb.Append(board.Board[r, c]);
                    sb.Append(',');
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Calculate the "degree" of a cell (number of empty neighbors in row/column/subgrid).
        /// </summary>
        private int CalculateDegree(SudokuBoard board, SolverState state, int r, int c)
        {
            int size = state.Size;
            var neighbors = new HashSet<(int, int)>();

            // same row
            for (int j = 0; j < size; j++)
            {
                if (j != c && board.Board[r, j] == 0)
                    neighbors.Add((r, j));
            }
            // same col
            for (int i = 0; i < size; i++)
            {
                if (i != r && board.Board[i, c] == 0)
                    neighbors.Add((i, c));
            }
            // same box
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
}
