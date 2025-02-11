using omegaSudoku.Board;

namespace omegaSudoku.Interfaces
{
    public interface IValidator
    {
        /// <summary>
        /// Checks the input format and calculates the board size (N for N×N).
        /// </summary>
        bool IsValidFormat(string input, out int boardSize);

        /// <summary>
        /// Checks that the board does not contain inconsistencies (duplicates in rows, columns, and subgrids).
        /// </summary>
        bool IsBoardValid(SudokuBoard board, int size);

        /// <summary>
        /// Checks that the board is solvable, meaning that the existing values do not contradict a solution.
        /// </summary>
        bool IsSolvable(SudokuBoard board, int size);
    }
}
