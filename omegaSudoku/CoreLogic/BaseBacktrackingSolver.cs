using System.Text;
using System.Numerics;
using System.Collections.Generic;
using omegaSudoku.BoardAndCells;
using omegaSudoku.Interfaces;

namespace omegaSudoku.CoreLogic
{
    public abstract class BaseBacktrackingSolver : ISudokuSolver
    {
        protected readonly IHeuristic[] _heuristics;
        protected TranspositionTable _transpositionTable;
        protected ConstraintChecker _constraintChecker;

        protected BaseBacktrackingSolver(IHeuristic[] heuristics)
        {
            _heuristics = heuristics;
            _transpositionTable = new TranspositionTable();
            _constraintChecker = new ConstraintChecker();
        }

        public abstract bool Solve(SudokuBoard board);

        /// <summary>
        /// Initializes row, column, and box masks according to the current board.
        /// Creates a new SolverState object with the correct bitmasks.
        /// </summary>
        protected bool InitializeState(SudokuBoard board, SolverState state)
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

        /// <summary>
        /// Checks if the current board configuration has already been encountered in the transposition table.
        /// </summary>
        protected bool AlreadySeenConfiguration(SudokuBoard board)
        {
            string hash = GetBoardHash(board);
            if (_transpositionTable.Contains(hash))
            {
                return true;
            }
            // If not seen before, add it now
            _transpositionTable.Add(hash);
            return false;
        }

        /// <summary>
        /// Returns a string representing the current board state for the transposition table.
        /// </summary>
        protected string GetBoardHash(SudokuBoard board)
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
    }
}
