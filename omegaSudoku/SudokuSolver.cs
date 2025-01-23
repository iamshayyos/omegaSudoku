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
                progress |= ApplyNakedPairs(candidates, size);
                progress |= ApplyLockedCandidates(candidates, size);
                progress |= ApplyHiddenTriplesQuads(candidates, size);

                if (!progress)
                    break;
            }

            return BacktrackingSolve(board, size);
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
                            if (IsValidMove(board, row, col, num, size))                             {
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

        private bool IsValidMove(int[,] board, int row, int col, int num, int size)
        {
            for (int i = 0; i < size; i++)
            {
                if (board[row, i] == num)
                    return false;
            }

            for (int i = 0; i < size; i++)
            {
                if (board[i, col] == num)
                    return false;
            }

            int subgridSize = (int)Math.Sqrt(size);
            int startRow = (row / subgridSize) * subgridSize;
            int startCol = (col / subgridSize) * subgridSize;

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


        private List<int>[,] InitializeCandidates(int[,] board, int size)
        {
            var candidates = new List<int>[size, size];
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (board[row, col] == 0)
                    {
                        candidates[row, col] = new List<int>();
                        for (int num = 1; num <= size; num++)
                        {
                            if (IsValidMove(board, row, col, num, size))
                                candidates[row, col].Add(num);
                        }
                    }
                    else
                    {
                        candidates[row, col] = null;
                    }
                }
            }
            return candidates;
        }

        private bool ApplyHiddenSingles(int[,] board, List<int>[,] candidates)
        {
            bool progress = false;

            for (int row = 0; row < candidates.GetLength(0); row++)
            {
                for (int col = 0; col < candidates.GetLength(1); col++)
                {
                    if (candidates[row, col] != null && candidates[row, col].Count == 1)
                    {
                        int value = candidates[row, col][0];
                        board[row, col] = value;
                        UpdateCandidates(candidates, board, row, col, value);
                        progress = true;
                    }
                }
            }

            return progress;
        }

        private bool ApplyUniquenessInUnit(int[,] board, List<int>[,] candidates, int size)
        {
            bool progress = false;
            int subgridSize = (int)Math.Sqrt(size);

            for (int unit = 0; unit < size; unit++)
            {
                for (int num = 1; num <= size; num++)
                {
                    var unitCells = GetUnitCells(unit, subgridSize, size);
                    var possibleCells = unitCells.Where(cell => candidates[cell.Item1, cell.Item2]?.Contains(num) == true).ToList();

                    if (possibleCells.Count == 1)
                    {
                        var (row, col) = possibleCells[0];
                        board[row, col] = num;
                        UpdateCandidates(candidates, board, row, col, num);
                        progress = true;
                    }
                }
            }

            return progress;
        }

        private bool ApplyNakedPairs(List<int>[,] candidates, int size)
        {
            bool progress = false;
            int subgridSize = (int)Math.Sqrt(size);

            for (int unit = 0; unit < size; unit++)
            {
                var unitCells = GetUnitCells(unit, subgridSize, size);
                var pairs = unitCells
                    .Where(cell => candidates[cell.Item1, cell.Item2]?.Count == 2)
                    .GroupBy(cell => string.Join(",", candidates[cell.Item1, cell.Item2]))
                    .Where(g => g.Count() == 2);

                foreach (var pair in pairs)
                {
                    var pairCandidates = candidates[pair.First().Item1, pair.First().Item2];
                    foreach (var cell in unitCells.Where(cell => !pair.Contains(cell)))
                    {
                        if (candidates[cell.Item1, cell.Item2] != null)
                        {
                            pairCandidates.ForEach(c => candidates[cell.Item1, cell.Item2].Remove(c));
                            progress = true;
                        }
                    }
                }
            }

            return progress;
        }

        private bool ApplyLockedCandidates(List<int>[,] candidates, int size)
        {
            bool progress = false;
            int subgridSize = (int)Math.Sqrt(size);

            for (int unit = 0; unit < size; unit++)
            {
                var unitCells = GetUnitCells(unit, subgridSize, size);
                var candidateOccurrences = new Dictionary<int, List<(int, int)>>();

                foreach (var cell in unitCells)
                {
                    if (candidates[cell.Item1, cell.Item2] != null)
                    {
                        foreach (var candidate in candidates[cell.Item1, cell.Item2])
                        {
                            if (!candidateOccurrences.ContainsKey(candidate))
                                candidateOccurrences[candidate] = new List<(int, int)>();
                            candidateOccurrences[candidate].Add(cell);
                        }
                    }
                }

                foreach (var kvp in candidateOccurrences)
                {
                    if (kvp.Value.All(c => c.Item1 == kvp.Value[0].Item1))
                    {
                        int lockedRow = kvp.Value[0].Item1;
                        foreach (var cell in GetRowCells(lockedRow, size).Except(unitCells))
                        {
                            if (candidates[cell.Item1, cell.Item2]?.Remove(kvp.Key) == true)
                                progress = true;
                        }
                    }

                    if (kvp.Value.All(c => c.Item2 == kvp.Value[0].Item2))
                    {
                        int lockedCol = kvp.Value[0].Item2;
                        foreach (var cell in GetColCells(lockedCol, size).Except(unitCells))
                        {
                            if (candidates[cell.Item1, cell.Item2]?.Remove(kvp.Key) == true)
                                progress = true;
                        }
                    }
                }
            }

            return progress;
        }

        private bool ApplyHiddenTriplesQuads(List<int>[,] candidates, int size)
        {
            return ApplyHiddenSubset(candidates, size, 3) || ApplyHiddenSubset(candidates, size, 4);
        }

        private bool ApplyHiddenSubset(List<int>[,] candidates, int size, int subsetSize)
        {
            bool progress = false;
            int subgridSize = (int)Math.Sqrt(size);

            for (int unit = 0; unit < size; unit++)
            {
                var unitCells = GetUnitCells(unit, subgridSize, size);
                var candidateOccurrences = new Dictionary<int, List<(int, int)>>();

                foreach (var cell in unitCells)
                {
                    if (candidates[cell.Item1, cell.Item2] != null)
                    {
                        foreach (var candidate in candidates[cell.Item1, cell.Item2])
                        {
                            if (!candidateOccurrences.ContainsKey(candidate))
                                candidateOccurrences[candidate] = new List<(int, int)>();
                            candidateOccurrences[candidate].Add(cell);
                        }
                    }
                }

                var subsets = GetCombinations(candidateOccurrences.Keys.ToList(), subsetSize);

                foreach (var subset in subsets)
                {
                    var subsetCells = subset.SelectMany(c => candidateOccurrences[c]).Distinct().ToList();

                    if (subsetCells.Count == subsetSize)
                    {
                        foreach (var cell in subsetCells)
                        {
                            candidates[cell.Item1, cell.Item2].RemoveAll(c => !subset.Contains(c));
                            progress = true;
                        }
                    }
                }
            }

            return progress;
        }

        private IEnumerable<(int, int)> GetUnitCells(int unit, int subgridSize, int size)
        {
            int startRow = (unit / subgridSize) * subgridSize;
            int startCol = (unit % subgridSize) * subgridSize;

            for (int row = 0; row < subgridSize; row++)
            {
                for (int col = 0; col < subgridSize; col++)
                {
                    yield return (startRow + row, startCol + col);
                }
            }
        }

        private IEnumerable<(int, int)> GetRowCells(int row, int size)
        {
            for (int col = 0; col < size; col++)
                yield return (row, col);
        }

        private IEnumerable<(int, int)> GetColCells(int col, int size)
        {
            for (int row = 0; row < size; row++)
                yield return (row, col);
        }

        private void UpdateCandidates(List<int>[,] candidates, int[,] board, int row, int col, int value)
        {
            int size = board.GetLength(0);
            int subgridSize = (int)Math.Sqrt(size);

            for (int i = 0; i < size; i++)
            {
                candidates[row, i]?.Remove(value);
                candidates[i, col]?.Remove(value);
            }

            int startRow = (row / subgridSize) * subgridSize;
            int startCol = (col / subgridSize) * subgridSize;

            for (int r = startRow; r < startRow + subgridSize; r++)
            {
                for (int c = startCol; c < startCol + subgridSize; c++)
                {
                    candidates[r, c]?.Remove(value);
                }
            }

            candidates[row, col] = null;
        }

        public static IEnumerable<List<T>> GetCombinations<T>(List<T> list, int length)
        {
            if (length == 0) return new List<List<T>> { new List<T>() };

            return list.SelectMany((item, index) =>
                GetCombinations(list.Skip(index + 1).ToList(), length - 1)
                    .Select(combination => new List<T> { item }.Concat(combination).ToList()));
        }
    }
}
