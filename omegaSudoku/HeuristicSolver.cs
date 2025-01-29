using System;
using System.Collections.Generic;

namespace omegaSudoku
{
    public class HeuristicSolver
    {
        private int[,] board;
        private int size;

        public HeuristicSolver(int[,] board, int size)
        {
            this.board = board;
            this.size = size;
        }

        public bool SolveUsingHeuristics()
        {
            var mostConstrainedCell = GetMostConstrainedCell();
            if (mostConstrainedCell == null)
            {
                return IsBoardComplete();
            }

            int row = mostConstrainedCell.Value.row;
            int col = mostConstrainedCell.Value.col;
            var candidates = GetCandidates(row, col);

            foreach (var candidate in candidates)
            {
                board[row, col] = candidate;

                if (SolveUsingHeuristics())
                {
                    return true;
                }

                board[row, col] = 0;
            }

            return false;
        }

        private (int row, int col)? GetMostConstrainedCell()
        {
            int minOptions = int.MaxValue;
            (int row, int col)? mostConstrainedCell = null;

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (board[r, c] != 0) continue;

                    var candidates = GetCandidates(r, c);
                    if (candidates.Count < minOptions)
                    {
                        minOptions = candidates.Count;
                        mostConstrainedCell = (r, c);
                    }

                    if (minOptions == 1) return mostConstrainedCell;
                }
            }

            return mostConstrainedCell;
        }

        private List<int> GetCandidates(int row, int col)
        {
            var candidates = new HashSet<int>();
            for (int num = 1; num <= size; num++)
            {
                candidates.Add(num);
            }

            for (int i = 0; i < size; i++)
            {
                candidates.Remove(board[row, i]);
                candidates.Remove(board[i, col]);
            }

            int subgridSize = (int)Math.Sqrt(size);
            int subgridRowStart = (row / subgridSize) * subgridSize;
            int subgridColStart = (col / subgridSize) * subgridSize;

            for (int r = 0; r < subgridSize; r++)
            {
                for (int c = 0; c < subgridSize; c++)
                {
                    candidates.Remove(board[subgridRowStart + r, subgridColStart + c]);
                }
            }

            return new List<int>(candidates);
        }

        private bool IsBoardComplete()
        {
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (board[r, c] == 0) return false;
                }
            }
            return true;
        }
    }
}
