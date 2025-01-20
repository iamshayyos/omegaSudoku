using System;
using System.Diagnostics;

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

                PrintMessage("Initial Sudoku board:");
                board.PrintBoard();

                Stopwatch stopwatch = Stopwatch.StartNew();
                bool isSolved = _solver.Solve(board.Board, boardSize);
                stopwatch.Stop();

                if (isSolved)
                {
                    PrintMessage("Solved Sudoku board:");
                    board.PrintBoard();
                }
                else
                {
                    PrintMessage("This Sudoku board is unsolvable.");
                }

                PrintMessage($"Time taken to solve: {stopwatch.ElapsedMilliseconds} ms");
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
    }
}
