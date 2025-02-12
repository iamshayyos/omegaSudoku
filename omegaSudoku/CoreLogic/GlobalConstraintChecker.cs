using omegaSudoku.Board;

namespace omegaSudoku.CoreLogic
{
    /// <summary>
    /// Ensures that each missing number in a Sudoku board has at least one valid placement.
    /// Helps detect unsolvable states early to optimize the solving process.
    /// </summary>
    public class GlobalConstraintChecker
    {
        /// <summary>
        /// Checks all global constraints (rows, columns, and boxes).
        /// Returns true if the board remains solvable, otherwise false.
        /// </summary>
        public bool CheckGlobalConstraints(SolverState state)
        {
            int size = state.Size;
            int subSize = state.SubSize;
            int fullMask = state.FullMask;
            int[,] board = state.BoardArray;

            for (int r = 0; r < size; r++)
            {
                int missing = fullMask & ~state.RowUsed[r];
                if (!IsValidConstraint(state, board, missing, r, isRow: true))
                    return false;
            }

            for (int c = 0; c < size; c++)
            {
                int missing = fullMask & ~state.ColUsed[c];
                if (!IsValidConstraint(state, board, missing, c, isRow: false))
                    return false;
            }

            for (int br = 0; br < subSize; br++)
            {
                for (int bc = 0; bc < subSize; bc++)
                {
                    int boxIndex = br * subSize + bc;
                    int missing = fullMask & ~state.BoxUsed[boxIndex];
                    if (!IsValidBoxConstraint(state, board, missing, br, bc))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Ensures that every missing digit in a row or column has at least one possible placement.
        /// </summary>
        private bool IsValidConstraint(SolverState state, int[,] board, int missing, int index, bool isRow)
        {
            if (missing == 0) return true;

            for (int digit = 1; digit <= state.Size; digit++)
            {
                int bit = 1 << (digit - 1);
                if ((missing & bit) != 0)
                {
                    bool found = false;
                    for (int i = 0; i < state.Size; i++)
                    {
                        int r = isRow ? index : i;
                        int c = isRow ? i : index;

                        if (board[r, c] == 0 && (state.GetCandidates(r, c) & bit) != 0)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Ensures that every missing digit in a box has at least one valid placement.
        /// </summary>
        private bool IsValidBoxConstraint(SolverState state, int[,] board, int missing, int br, int bc)
        {
            if (missing == 0) return true;

            for (int digit = 1; digit <= state.Size; digit++)
            {
                int bit = 1 << (digit - 1);
                if ((missing & bit) != 0)
                {
                    bool found = false;
                    for (int r = br * state.SubSize; r < (br + 1) * state.SubSize; r++)
                    {
                        for (int c = bc * state.SubSize; c < (bc + 1) * state.SubSize; c++)
                        {
                            if (board[r, c] == 0 && (state.GetCandidates(r, c) & bit) != 0)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found) return false;
                }
            }
            return true;
        }
    }
}
