using System;
using System.Collections.Generic;
using System.Numerics;
using omegaSudoku.BoardAndCells;
using omegaSudoku.Interfaces;

namespace omegaSudoku.CoreLogic
{
    public class Advanced25x25Solver : BaseBacktrackingSolver
    {
        public Advanced25x25Solver(IHeuristic[] heuristics)
            : base(heuristics)
        {
        }

        public override bool Solve(SudokuBoard board)
        {
            SolverState state = new SolverState(board.Size);
            if (!InitializeState(board, state))
                return false;

            // Compute candidates for each empty cell
            int size = state.Size;
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

            // 1. Select the subgrid with the lowest average number of candidates
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
                        if (avg < bestAverage)
                        {
                            bestAverage = avg;
                            bestBr = br;
                            bestBc = bc;
                        }
                    }
                }
            }

            // 2. Collect empty cells in the selected subgrid with candidate and degree information
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

            // 3. Sort by CandidateCount (ascending) -> then by Degree (descending)

            subgridCells.Sort((a, b) =>
            {
                int cmp = a.CandidateCount.CompareTo(b.CandidateCount);
                if (cmp == 0)
                    cmp = b.Degree.CompareTo(a.Degree);
                return cmp;
            });

            // 4. Enter Backtrack Subgrid
            return BacktrackSubgrid(board, state, subgridCells);
        }

        private bool BacktrackSubgrid(SudokuBoard board, SolverState state, List<CellInfo> subgridCells)
        {
            // If we have already seen this configuration, return false
            if (AlreadySeenConfiguration(board))
                return false;

            // If there are no more cells in the subgrid, proceed to standard Backtracking on the rest of the board
            if (subgridCells.Count == 0)
            {
                return BacktrackOptimized(board, state);
            }

            // Pick the first cell from the list
            var cell = subgridCells[0];
            subgridCells.RemoveAt(0);

            int r = cell.Row;
            int c = cell.Col;
            int candidateMask = state.FullMask & ~(state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)]);

            while (candidateMask != 0)
            {
                int bit = candidateMask & -candidateMask;
                candidateMask -= bit;
                int val = BitOperations.TrailingZeroCount((uint)bit) + 1;

                // Assign value

                board.Board[r, c] = val;
                state.RowUsed[r] |= bit;
                state.ColUsed[c] |= bit;
                state.BoxUsed[state.GetBoxIndex(r, c)] |= bit;

                if (_constraintChecker.ForwardCheck(board, state))
                {
                    if (BacktrackSubgrid(board, state, subgridCells))
                        return true;
                }

                // Undo assignment
                board.Board[r, c] = 0;
                state.RowUsed[r] &= ~bit;
                state.ColUsed[c] &= ~bit;
                state.BoxUsed[state.GetBoxIndex(r, c)] &= ~bit;
            }

            // Return the cell to the list
            subgridCells.Insert(0, cell);
            return false;
        }

        /// <summary>
        /// Backtracking using (MRV)
        /// </summary>
        private bool BacktrackOptimized(SudokuBoard board, SolverState state)
        {
            if (AlreadySeenConfiguration(board))
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
                            return false;
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
                if (bestCandidateCount == 1) break;
            }

            if (bestRow == -1)
                return true;

            while (bestCandidates != 0)
            {
                int bit = bestCandidates & -bestCandidates;
                bestCandidates -= bit;
                int val = BitOperations.TrailingZeroCount((uint)bit) + 1;

                board.Board[bestRow, bestCol] = val;
                state.RowUsed[bestRow] |= bit;
                state.ColUsed[bestCol] |= bit;
                state.BoxUsed[state.GetBoxIndex(bestRow, bestCol)] |= bit;

                if (_constraintChecker.ForwardCheck(board, state))
                {
                    if (BacktrackOptimized(board, state))
                        return true;
                }

                board.Board[bestRow, bestCol] = 0;
                state.RowUsed[bestRow] &= ~bit;
                state.ColUsed[bestCol] &= ~bit;
                state.BoxUsed[state.GetBoxIndex(bestRow, bestCol)] &= ~bit;
            }
            return false;
        }

        /// <summary>
        /// Calculate the "degree" of a cell 
        /// (number of empty neighbors in row/column/subgrid).
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
