using System;

namespace omegaSudoku
{
    public class SudokuSolverManager
    {
        private byte[,] board;
        private int size;

        public SudokuSolverManager(byte[,] board, int size)
        {
            this.board = board;
            this.size = size;
        }

        public bool Solve()
        {
            // Map the difficulty level of the board
            DifficultyMapper difficultyMapper = new DifficultyMapper();
            var difficulty = difficultyMapper.MapDifficulty(board, size);

            Console.WriteLine($"Detected difficulty: {difficulty}");

            bool isSolved = false;

            // Always start with heuristic solver
            Console.WriteLine("Using heuristic solver...");
            HeuristicSolver heuristicSolver = new HeuristicSolver(ConvertToIntBoard(), size);
            isSolved = heuristicSolver.SolveUsingHeuristics();

            // If heuristic solver doesn't finish the solution, use backtracking
            if (!IsBoardComplete())
            {
                Console.WriteLine("Using backtracking solver for finalization...");
                BacktrackingSolver backtrackingSolver = new BacktrackingSolver(ConvertToIntBoard(), size);
                isSolved = backtrackingSolver.SolveUsingBacktracking();
            }

            // Update the byte board if solved
            if (isSolved)
            {
                UpdateBoard(ConvertToIntBoard());
            }

            return isSolved;
        }

        private void UpdateBoard(int[,] intBoard)
        {
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    board[r, c] = (byte)intBoard[r, c];
                }
            }
        }

        private int[,] ConvertToIntBoard()
        {
            int[,] intBoard = new int[size, size];
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    intBoard[r, c] = board[r, c];
                }
            }
            return intBoard;
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
