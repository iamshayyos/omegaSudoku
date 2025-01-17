using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            //input
            string input = GetInput();

            // validation
            if (!_validator.IsValidFormat(input))
            {
                PrintMessage("Invalid input format. Make sure the input represents a valid Sudoku board.");
                return;
            }

            // generate board
            int boardSize = (int)Math.Sqrt(input.Length);
            SudokuBoard board = new SudokuBoard(input, boardSize);

            // print initial board
            PrintMessage("Initial Sudoku board:");
            board.PrintBoard();

            // solving the board
            if (_solver.Solve(board.Board, boardSize))
            {
                PrintMessage("Solved Sudoku board:");
                board.PrintBoard();
            }
            else
            {
                PrintMessage("This Sudoku board is unsolvable.");
            }
        }

        private string GetInput()
        {
            Console.WriteLine("Enter the Sudoku board as a single string:");
            return Console.ReadLine();
        }

        private void PrintMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
