using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace omegaSudoku
{
    public class IOHandler
    {
        private Validator _validator;
        private SudokuSolver _solver;

        public IOHandler()
        {
            _validator = new Validator();
            _solver = new SudokuSolver();
        }

        public void Run()
        {
            Console.WriteLine("Welcome to Omega Sudoku!");
            Console.WriteLine("Enter 'end' to exit the game.");

            while (true)
            {
                string input = GetInput();

                if (input.ToLower() == "end")
                {
                    Console.WriteLine("Exiting the program. Goodbye!");
                    break;
                }

                if (!_validator.IsValidFormat(input))
                {
                    PrintMessage("Invalid input format. Make sure the input represents a valid Sudoku board.");
                    continue;
                }

                int boardSize = (int)Math.Sqrt(input.Length);
                SudokuBoard board = new SudokuBoard(input, boardSize);

                if (!_validator.IsBoardValid(board.Board, boardSize))
                {
                    PrintMessage("The Sudoku board is invalid and cannot be solved. Please try again.");
                    continue;
                }

                if (!_validator.IsSolvable(board.Board, boardSize))
                {
                    PrintMessage("The Sudoku board is not solvable. Please try again.");
                    continue;
                }

                PrintMessage("Initial Sudoku board:");
                board.PrintBoard();

                Stopwatch stopwatch = Stopwatch.StartNew();
                bool isSolved;

                try
                {
                    isSolved = SolveWithTimeout(board, boardSize, TimeSpan.FromSeconds(10));
                }
                catch (TimeoutException)
                {
                    PrintMessage("The Sudoku board took too long to solve and is considered unsolvable.");
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
                    PrintMessage("This Sudoku board is unsolvable.");
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
                isSolved = _solver.Solve(board.Board, boardSize);
            });

            if (!solveTask.Wait(timeout))
            {
                throw new TimeoutException("The solution took too long and was terminated.");
            }

            return isSolved;
        }
    }
}
