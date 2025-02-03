using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using omegaSudoku;

namespace OmegaSudoku
{
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

            // Initialize the masks and empty board cells
            if (!InitializeState(board, state))
            {
                return false;
            }

            BuildEmptyCellsList(board, state);

            // Apply heuristics until no changes are made
            bool progress;
            do
            {
                progress = false;
                foreach (var heuristic in _heuristics)
                {
                    if (heuristic.Apply(board, state))
                    {
                        progress = true;
                        // Update the list of empty cells after each change
                        BuildEmptyCellsList(board, state);
                    }
                }
            } while (progress);
            // Search with Backtracking
            return Backtrack(board, state);
        }

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
                        {
                            return false; 
                        }
                        state.RowUsed[r] |= bit;
                        state.ColUsed[c] |= bit;
                        state.BoxUsed[boxIndex] |= bit;
                    }
                }
            }
            return true;
        }

        private void BuildEmptyCellsList(SudokuBoard board, SolverState state)
        {
            state.EmptyCells.Clear();
            for (int r = 0; r < state.Size; r++)
            {
                for (int c = 0; c < state.Size; c++)
                {
                    if (board.Board[r, c] == 0)
                    {
                        int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                        int possible = state.FullMask & ~used;
                        int options = BitUtils.PopCount(possible);
                        state.EmptyCells.Add(new Cell(r, c, options));
                    }
                }
            }
            // Sort by how many options each cell has (MRV heuristic)
            state.EmptyCells.Sort((a, b) => a.Options.CompareTo(b.Options));
        }

        private bool Backtrack(SudokuBoard board, SolverState state)
        {
            if (state.EmptyCells.Count == 0)
                return true;

            // Select the cell with the fewest possible choices (MRV)
            Cell cell = state.EmptyCells[0];
            state.EmptyCells.RemoveAt(0);

            int r = cell.Row;
            int c = cell.Col;
            int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
            int candidates = state.FullMask & ~used;

            while (candidates != 0)
            {
                int bit = candidates & -candidates; // Choose the lowest bit
                candidates &= candidates - 1;
                int val = BitUtils.TrailingZeroCount(bit) + 1;

                board.Board[r, c] = val;
                //Update masks
                state.RowUsed[r] |= bit;
                state.ColUsed[c] |= bit;
                state.BoxUsed[state.GetBoxIndex(r, c)] |= bit;
                // Backup the list of empty cells for fallback in case of failure
                var backupEmptyCells = new List<Cell>(state.EmptyCells);
                if (Backtrack(board, state))
                    return true;
                // Go back – cancel changes
                board.Board[r, c] = 0;
                state.RowUsed[r] &= ~bit;
                state.ColUsed[c] &= ~bit;
                state.BoxUsed[state.GetBoxIndex(r, c)] &= ~bit;
                state.EmptyCells = backupEmptyCells;
            }

            // Return the cell to the list in case you need to try more options
            state.EmptyCells.Insert(0, cell);
            return false;
        }
    }
}
