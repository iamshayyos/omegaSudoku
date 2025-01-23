using System;
using System.Collections.Generic;
using System.Linq;

namespace omegaSudoku
{
    public class SudokuSolver
    {
        public bool Solve(int[,] board, int size)
        {
            var candidates = InitializeCandidates(board, size);

            while (true)
            {
                bool progress = false;

                progress |= ApplyHiddenSingles(board, candidates);
                progress |= ApplyUniquenessInUnit(board, candidates, size);
                progress |= ApplyNakedPairs(board, candidates, size);

                if (!progress)
                    break;
            }

            return BacktrackingSolve(board, size);
        }

        private Dictionary<(int, int), List<int>> InitializeCandidates(int[,] board, int size)
        {
            var candidates = new Dictionary<(int, int), List<int>>();
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (board[row, col] == 0)
                    {
                        candidates[(row, col)] = new List<int>();
                        for (int num = 1; num <= size; num++)
                        {
                            if (IsValidMove(board, row, col, num, size))
                                candidates[(row, col)].Add(num);
                        }
                    }
                }
            }
            return candidates;
        }

        private bool ApplyHiddenSingles(int[,] board, Dictionary<(int, int), List<int>> candidates)
        {
            bool progress = false;
            foreach (var entry in candidates)
            {
                if (entry.Value.Count == 1)
                {
                    var (row, col) = entry.Key;
                    board[row, col] = entry.Value[0];
                    UpdateCandidates(candidates, board, board.GetLength(0), row, col, entry.Value[0]);
                    progress = true;
                }
            }
            return progress;
        }

        private bool ApplyUniquenessInUnit(int[,] board, Dictionary<(int, int), List<int>> candidates, int size)
        {
            bool progress = false;

            var candidateKeys = new List<(int, int)>(candidates.Keys);
            foreach (var key in candidateKeys)
            {
                var (row, col) = key;
                var cellCandidates = candidates[key];

                foreach (var num in cellCandidates)
                {
                    if (IsUniqueInUnit(candidates, row, col, num, size))
                    {
                        board[row, col] = num;
                        candidates.Remove((row, col));
                        UpdateCandidates(candidates, board, size, row, col, num);
                        progress = true;
                        break;
                    }
                }
            }

            return progress;
        }

        private bool IsUniqueInUnit(Dictionary<(int, int), List<int>> candidates, int row, int col, int num, int size)
        {
            foreach (var entry in candidates)
            {
                var (r, c) = entry.Key;
                if ((r == row || c == col || (r / (int)Math.Sqrt(size) == row / (int)Math.Sqrt(size) && c / (int)Math.Sqrt(size) == col / (int)Math.Sqrt(size))) &&
                    (r != row || c != col) &&
                    entry.Value.Contains(num))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ApplyNakedPairs(int[,] board, Dictionary<(int, int), List<int>> candidates, int size)
        {
            bool progress = false;

            foreach (var entry1 in candidates)
            {
                foreach (var entry2 in candidates)
                {
                    if (entry1.Key != entry2.Key && entry1.Value.Count == 2 && entry1.Value.SequenceEqual(entry2.Value))
                    {
                        var (row1, col1) = entry1.Key;
                        var (row2, col2) = entry2.Key;

                        foreach (var entry3 in candidates)
                        {
                            if (entry3.Key != entry1.Key && entry3.Key != entry2.Key)
                            {
                                var (row3, col3) = entry3.Key;
                                if (row1 == row3 || col1 == col3 || (row1 / (int)Math.Sqrt(size) == row3 / (int)Math.Sqrt(size) && col1 / (int)Math.Sqrt(size) == col3 / (int)Math.Sqrt(size)))
                                {
                                    foreach (var num in entry1.Value)
                                    {
                                        if (entry3.Value.Contains(num))
                                        {
                                            entry3.Value.Remove(num);
                                            progress = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return progress;
        }

        private bool BacktrackingSolve(int[,] board, int size)
        {
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (board[row, col] == 0)
                    {
                        for (int num = 1; num <= size; num++)
                        {
                            if (IsValidMove(board, row, col, num, size))
                            {
                                board[row, col] = num;
                                if (BacktrackingSolve(board, size))
                                    return true;
                                board[row, col] = 0;
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        private void UpdateCandidates(Dictionary<(int, int), List<int>> candidates, int[,] board, int size, int row, int col, int num)
        {
            var candidateKeys = new List<(int, int)>(candidates.Keys);
            foreach (var key in candidateKeys)
            {
                var (r, c) = key;
                if (r == row || c == col || (r / (int)Math.Sqrt(size) == row / (int)Math.Sqrt(size) && c / (int)Math.Sqrt(size) == col / (int)Math.Sqrt(size)))
                {
                    candidates[key].Remove(num);
                    if (candidates[key].Count == 0)
                        candidates.Remove(key);
                }
            }
        }

        private bool IsValidMove(int[,] board, int row, int col, int num, int size)
        {
            for (int i = 0; i < size; i++)
            {
                if (board[row, i] == num || board[i, col] == num)
                    return false;
            }

            int subgridSize = (int)Math.Sqrt(size);
            int startRow = row / subgridSize * subgridSize;
            int startCol = col / subgridSize * subgridSize;

            for (int r = startRow; r < startRow + subgridSize; r++)
            {
                for (int c = startCol; c < startCol + subgridSize; c++)
                {
                    if (board[r, c] == num)
                        return false;
                }
            }
            return true;
        }
    }
}
