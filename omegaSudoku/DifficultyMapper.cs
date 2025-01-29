using System;
using System.Linq;
using System.Collections.Generic;

namespace omegaSudoku
{
	public class DifficultyMapper
	{
		public enum DifficultyLevel
		{
			Easy,
			Medium,
			Hard,
			Expert
		}

		public DifficultyLevel MapDifficulty(byte[,] board, int size)
		{
			int emptyCells = CountEmptyCells(board, size);
			int constraints = CalculateConstraints(board, size);

			if (emptyCells <= size && constraints >= size * 3)
				return DifficultyLevel.Easy;
			else if (emptyCells <= size * 2)
				return DifficultyLevel.Medium;
			else if (emptyCells <= size * 3)
				return DifficultyLevel.Hard;
			else
				return DifficultyLevel.Expert;
		}

		private int CountEmptyCells(byte[,] board, int size)
		{
			int count = 0;
			for (int r = 0; r < size; r++)
			{
				for (int c = 0; c < size; c++)
				{
					if (board[r, c] == 0) count++;
				}
			}
			return count;
		}

		private int CalculateConstraints(byte[,] board, int size)
		{
			int constraints = 0;
			for (int r = 0; r < size; r++)
			{
				for (int c = 0; c < size; c++)
				{
					if (board[r, c] == 0)
					{
						constraints += GetCandidates(r, c, board, size).Count;
					}
				}
			}
			return constraints;
		}

		private List<int> GetCandidates(int row, int col, byte[,] board, int size)
		{
			var candidates = new HashSet<int>(Enumerable.Range(1, size));

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

			return candidates.ToList();
		}
	}
}
