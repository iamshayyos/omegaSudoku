using omegaSudoku;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace omegaSudoku
{
    public class IOHandler
    {
        private readonly ISudokuSolver _solver;
        private readonly IValidator _validator;

        public IOHandler(ISudokuSolver solver, IValidator validator)
        {
            _solver = solver;
            _validator = validator;
        }

        public void Run()
        {
            Console.WriteLine("Welcome to Omega Sudoku!");
            Console.WriteLine("Enter 'end' to exit the game.");

            while (true)
            {
                string input = GetInput();
                if (input == null) return;
                if (input.Trim().ToLower() == "end")
                {
                    Console.WriteLine("Exiting the program. Goodbye!");
                    break;
                }

                if (!_validator.IsValidFormat(input, out int boardSize))
                {
                    PrintMessage("Invalid input format. Make sure the input is a square (N*N) with N in [1..25].");
                    continue;
                }

                //create board
                SudokuBoard board = new SudokuBoard(input, boardSize);

                if (!_validator.IsBoardValid(board, boardSize))
                {
                    PrintMessage("The Sudoku board is invalid (conflicting values). Please try again.");
                    continue;
                }

                if (!_validator.IsSolvable(board, boardSize))
                {
                    PrintMessage("The Sudoku board has no solution (immediate contradiction). Try again.");
                    continue;
                }

                PrintMessage("Initial Sudoku board:");
                board.PrintBoard();

                var stopwatch = Stopwatch.StartNew();
                bool isSolved = false;

                try
                {
                    TimeSpan timeout = TimeSpan.FromSeconds(10);
                    isSolved = SolveWithTimeout(board, boardSize, timeout);
                }
                catch (TimeoutException)
                {
                    PrintMessage("The Sudoku board took too long to solve and is considered unsolvable for now.");
                    continue;
                }
                stopwatch.Stop();

                if (isSolved)
                {
                    PrintMessage("Solved Sudoku board:");
                    board.PrintBoard();
                    PrintMessage($"Time taken to solve: {stopwatch.ElapsedMilliseconds} ms");
                }
                else
                {
                    PrintMessage("This Sudoku board is unsolvable or timed out.");
                }
            }
        }

        private string GetInput()
        {
            Console.WriteLine("\nEnter the Sudoku board as a single string (or 'end' to exit):");
            return Console.ReadLine();
        }

        private void PrintMessage(string message)
        {
            Console.WriteLine(message);
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
