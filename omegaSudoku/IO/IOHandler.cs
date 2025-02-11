using System;
using System.Diagnostics;
using System.IO;
using omegaSudoku.Board;
using omegaSudoku.Exceptions;
using omegaSudoku.Interfaces;

namespace omegaSudoku.IO
{
    /// <summary>
    /// Handles all input/output operations for the Sudoku application.
    /// </summary>
    public class IOHandler
    {
        private readonly ISudokuSolver _solver;
        private readonly IValidator _validator;
        private readonly SudokuInputProvider _inputProvider;
        private bool _exitRequested = false;

        /// <summary>
        /// Initializes a new instance of IOHandler and sets up the Ctrl+C handler.
        /// </summary>
        public IOHandler(ISudokuSolver solver, IValidator validator)
        {
            _solver = solver;
            _validator = validator;
            _inputProvider = new SudokuInputProvider();

            // Handle Ctrl+C input for graceful termination.
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("\nCtrl+C detected. Do you really want to exit? (y/n)");
                string response = Console.ReadLine()?.Trim().ToLower();
                if (response == "y")
                {
                    _exitRequested = true;
                    Console.WriteLine("Exiting program...");
                }
                else
                {
                    e.Cancel = true; // Cancel exit if user decides not to quit.
                    Console.WriteLine("Continuing...");
                }
            };
        }

        /// <summary>
        /// Main execution loop for reading input, validating, solving, and displaying the Sudoku board.
        /// </summary>
        public void Run()
        {
            Console.WriteLine("Welcome to Omega Sudoku!");
            Console.WriteLine("Enter 'end' to exit.");

            while (!_exitRequested)
            {
                string input;
                try
                {
                    // Get puzzle input from the user.
                    input = _inputProvider.GetPuzzleInput();
                    if (input == null)
                    {
                        Console.WriteLine("\nCtrl+Z detected (EOF). Exiting program...");
                        break;
                    }
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

                // Exit command received.
                if (input == "end")
                {
                    Console.WriteLine("Exiting the program. Goodbye!");
                    return;
                }

                SudokuBoard board = null;
                int boardSize = 0;
                try
                {
                    _validator.IsValidFormat(input, out boardSize);
                    board = new SudokuBoard(input, boardSize);
                    _validator.IsBoardValid(board, boardSize);
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

                Console.WriteLine("Initial Sudoku board:");
                board.PrintBoard();

                var stopwatch = Stopwatch.StartNew();
                bool isSolved = false;

                try
                {
                    isSolved = _solver.Solve(board);
                }
                catch (UnsolvableBoardException ex)
                {
                    Console.WriteLine("Unsolvable board: " + ex.Message);
                    continue;
                }

                stopwatch.Stop();

                if (isSolved)
                {
                    Console.WriteLine("Solved Sudoku board:");
                    board.PrintBoard();
                    Console.WriteLine($"Time taken to solve: {stopwatch.ElapsedMilliseconds} ms");
                }
            }
        }
    }
}
