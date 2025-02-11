using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using omegaSudoku.BoardAndCells;
using omegaSudoku.Exceptions;
using omegaSudoku.Interfaces;

namespace omegaSudoku.IO
{
    /// <summary>
    /// Handles console I/O for the Sudoku application.
    /// </summary>
    public class IOHandler
    {
        private readonly ISudokuSolver _solver;
        private readonly IValidator _validator;
        private readonly SudokuInputProvider _inputProvider;

        public IOHandler(ISudokuSolver solver, IValidator validator)
        {
            _solver = solver;
            _validator = validator;
            _inputProvider = new SudokuInputProvider();
        }

        public void Run()
        {
            Console.WriteLine("Welcome to Omega Sudoku!");
            Console.WriteLine("Enter 'end' to exit.");

            while (true)
            {
                string input;
                try
                {
                    input = _inputProvider.GetPuzzleInput();

                }
                catch (InvalidInputException ex)
                {
                    Console.WriteLine($"Input error: {ex.Message}");
                    continue;
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine($"File error: {ex.Message}");
                    continue;
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"File reading error: {ex.Message}");
                    continue;
                }
                // If user typed "end", we exit
                if (input == "end")
                {
                    Console.WriteLine("Exiting the program. Goodbye!");
                    return;
                }

               


                SudokuBoard board = null;
                int boardSize = 0;
                try
                {
                    // Validate format
                    _validator.IsValidFormat(input, out boardSize);

                    // Create the board
                    board = new SudokuBoard(input, boardSize);

                    // Validate board consistency
                    _validator.IsBoardValid(board, boardSize);

                    // Check solvability
                    _validator.IsSolvable(board, boardSize);
                }
                catch (InputTooShortException ex)
                {
                    Console.WriteLine("Input format error: " + ex.Message);
                    continue;
                }
                catch (InputTooLargeException ex)
                {
                    Console.WriteLine("Input format error: " + ex.Message);
                    continue;
                }
                catch (InvalidCharacterException ex)
                {
                    Console.WriteLine("Input format error: " + ex.Message);
                    continue;
                }
                catch (InvalidFormatException ex)
                {
                    Console.WriteLine("Input format error: " + ex.Message);
                    continue;
                }
                catch (BoardInitializationException ex)
                {
                    Console.WriteLine("Board initialization error: " + ex.Message);
                    continue;
                }
                catch (UnsolvableBoardException ex)
                {
                    Console.WriteLine("Unsolvable board: " + ex.Message);
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
                    TimeSpan timeout = TimeSpan.FromSeconds(1);
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

            }
        }

        private bool SolveWithTimeout(SudokuBoard board, int boardSize, TimeSpan timeout)
        {
            bool isSolved = false;
            Task solveTask = Task.Run(() =>
            {
                try
                {
                    isSolved = _solver.Solve(board);
                }
                catch (UnsolvableBoardException ex)
                {
                    Console.WriteLine("Unsolvable board: " + ex.Message);
                }

            });

            if (!solveTask.Wait(timeout))
            {
                throw new TimeoutException("The solution took too long and was terminated.");
            }

            return isSolved;
        }
    }
}
