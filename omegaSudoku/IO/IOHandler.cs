using System;
using System.Diagnostics;
using System.Threading.Tasks;
using omegaSudoku.BoardAndCells;
using omegaSudoku.Interfaces;

namespace omegaSudoku.IO
{
    public class IOHandler
    {
        private readonly ISudokuSolver _solver;
        private readonly IValidator _validator;

        // We add a private field for SudokuInputProvider
        private readonly SudokuInputProvider _inputProvider;

        public IOHandler(ISudokuSolver solver, IValidator validator)
        {
            _solver = solver;
            _validator = validator;

            // Instantiate our new provider
            _inputProvider = new SudokuInputProvider();
        }

        public void Run()
        {
            Console.WriteLine("Welcome to Omega Sudoku!");
            Console.WriteLine("Enter 'end' to exit.");

            while (true)
            {
                // Instead of inlining the input logic, we call _inputProvider.GetPuzzleInput()
                string input = _inputProvider.GetPuzzleInput();

                // If user typed "end", we exit
                if (input == "end")
                {
                    Console.WriteLine("Exiting the program. Goodbye!");
                    return;
                }

                // If null, it means invalid choice or read error → prompt again
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("No valid puzzle input was provided. Please try again.");
                    continue;
                }

                // Validate format
                if (!_validator.IsValidFormat(input, out int boardSize))
                {
                    Console.WriteLine("Invalid input format. The length must be N*N (where N in [1..25]).");
                    continue;
                }

                // Create the board
                SudokuBoard board = new SudokuBoard(input, boardSize);

                // Validate the board's internal consistency
                if (!_validator.IsBoardValid(board, boardSize))
                {
                    Console.WriteLine("The Sudoku board is invalid (conflicting values). Please try again.");
                    continue;
                }

                // Check that it's solvable (no immediate contradictions)
                if (!_validator.IsSolvable(board, boardSize))
                {
                    Console.WriteLine("The Sudoku board has no solution (immediate contradiction). Try again.");
                    continue;
                }

                // Print initial board
                Console.WriteLine("Initial Sudoku board:");
                board.PrintBoard();

                // Attempt to solve
                var stopwatch = Stopwatch.StartNew();
                bool isSolved = false;

                try
                {
                    TimeSpan timeout = TimeSpan.FromSeconds(10);
                    isSolved = SolveWithTimeout(board, boardSize, timeout);
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("The Sudoku board took too long to solve. Considered unsolvable for now.");
                    continue;
                }

                stopwatch.Stop();

                // Print results
                if (isSolved)
                {
                    Console.WriteLine("Solved Sudoku board:");
                    board.PrintBoard();
                    Console.WriteLine($"Time taken to solve: {stopwatch.ElapsedMilliseconds} ms");
                }
                else
                {
                    Console.WriteLine("This Sudoku board is unsolvable or timed out.");
                }
            }
        }

        private bool SolveWithTimeout(SudokuBoard board, int boardSize, TimeSpan timeout)
        {
            bool isSolved = false;
            Task solveTask = Task.Run(() =>
            {
                isSolved = _solver.Solve(board);
            });

            if (!solveTask.Wait(timeout))
            {
                throw new TimeoutException("The solution took too long and was terminated.");
            }

            return isSolved;
        }
    }
}
