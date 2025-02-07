using omegaSudoku.BoardAndCells;
using System.Numerics;

namespace omegaSudoku.CoreLogic
{
    public class ConstraintChecker
    {
        public bool ForwardCheck(SudokuBoard board, SolverState state)
        {
            // 1) Zero-candidate check: Ensures all empty cells have at least one candidate
            for (int r = 0; r < state.Size; r++)
            {
                for (int c = 0; c < state.Size; c++)
                {
                    if (board.Board[r, c] == 0)
                    {
                        int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                        int candidates = state.FullMask & ~used;
                        if (candidates == 0)
                            return false;// No candidates -> failure
                    }
                }
            }
            // 2) Missing number check (ensures every row, column, and box can still contain all required numbers)
            return CheckRowColBoxMissing(board, state);
        }

        private bool CheckRowColBoxMissing(SudokuBoard board, SolverState state)
        {
            int size = state.Size;
            int full = state.FullMask;
            int subSize = state.SubSize;

            // Check rows
            for (int r = 0; r < size; r++)
            {
                int rowUsed = state.RowUsed[r];
                for (int digit = 1; digit <= size; digit++)
                {
                    int bit = 1 << (digit - 1);
                    if ((rowUsed & bit) == 0) // digit not in row
                    {
                        bool canPlace = false;
                        for (int c = 0; c < size && !canPlace; c++)
                        {
                            if (board.Board[r, c] == 0)
                            {
                                int used = state.RowUsed[r] | state.ColUsed[c] | state.BoxUsed[state.GetBoxIndex(r, c)];
                                if ((used & bit) == 0) // still possible
                                    canPlace = true;
                            }
                        }
                        if (!canPlace) return false;
                    }
                }
            }

            // Check columns
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

            // Check subgrids
            for (int boxRow = 0; boxRow < size; boxRow += subSize)
            {
                for (int boxCol = 0; boxCol < size; boxCol += subSize)
                {
                    int boxUsed = 0;
                    for (int r = boxRow; r < boxRow + subSize; r++)
                    {
                        for (int c = boxCol; c < boxCol + subSize; c++)
                        {
                            int val = board.Board[r, c];
                            if (val != 0)
                                boxUsed |= 1 << (val - 1);
                        }
                    }
                    for (int digit = 1; digit <= size; digit++)
                    {
                        int bit = 1 << (digit - 1);
                        if ((boxUsed & bit) == 0)
                        {
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
    }
}
