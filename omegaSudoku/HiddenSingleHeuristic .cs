using omegaSudoku;

namespace omegaSudoku
{
    public class HiddenSingleHeuristic : IHeuristic
    {
        public bool Apply(SudokuBoard board, SolverState state)
        {
            bool changed = false;
            for (int i = 0; i < state.Size; i++)
            {
                changed |= ApplyHiddenSingle(board, state, i, true);  // Process rows.
                changed |= ApplyHiddenSingle(board, state, i, false); // Process columns.
            }
            return changed;
        }

        private bool ApplyHiddenSingle(SudokuBoard board, SolverState state, int index, bool isRow)
        {
            bool changed = false;
            int[] counts = new int[state.Size]; // Count occurrences for each candidate.
            int[,] positions = new int[state.Size, 2]; // Store the position for each candidate.

            // Count candidate occurrences in the unit (row or column).
            for (int i = 0; i < state.Size; i++)
            {
                int row = isRow ? index : i;
                int col = isRow ? i : index;

                if (board.Board[row, col] == 0)
                {
                    int used = state.RowUsed[row] | state.ColUsed[col] | state.BoxUsed[state.GetBoxIndex(row, col)];
                    int possible = state.FullMask & ~used;
                    for (int j = 0; j < state.Size; j++)
                    {
                        if ((possible & (1 << j)) != 0)
                        {
                            counts[j]++;
                            positions[j, 0] = row;
                            positions[j, 1] = col;
                        }
                    }
                }
            }

            // For any candidate that appears exactly once, fill it in.
            for (int val = 0; val < state.Size; val++)
            {
                if (counts[val] == 1)
                {
                    int row = positions[val, 0];
                    int col = positions[val, 1];
                    int bit = 1 << val;

                    board.Board[row, col] = val + 1;
                    state.RowUsed[row] |= bit;
                    state.ColUsed[col] |= bit;
                    state.BoxUsed[state.GetBoxIndex(row, col)] |= bit;
                    changed = true;
                }
            }

            return changed;
        }
    }
}
