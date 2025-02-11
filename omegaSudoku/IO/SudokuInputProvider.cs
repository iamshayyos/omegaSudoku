using System;
using System.IO;
using omegaSudoku.Exceptions;

namespace omegaSudoku.IO
{
    /// <summary>
    /// Handles the logic for retrieving a Sudoku puzzle string 
    /// from either console input or from a file.
    /// </summary>
    public class SudokuInputProvider
    {
        /// <summary>
        /// Displays a menu to the user, asks for the puzzle input source 
        /// (manual vs file), and returns the puzzle string or null if invalid.
        /// Returns "end" if the user wants to exit.
        /// </summary>
        public string GetPuzzleInput()
        {
            Console.WriteLine("\nChoose an option:");
            Console.WriteLine("1) Enter Sudoku puzzle manually (as a single string)");
            Console.WriteLine("2) Read Sudoku puzzle from a txt file");
            Console.WriteLine("Or type 'end' to exit.");

            string choice = Console.ReadLine()?.Trim().ToLower();

            if (choice == "end")
            {
                //user wants to exit
                return "end";
            }
            else if (choice == "1")
            {
                // Manual input
                return GetManualInput();
            }
            else if (choice == "2")
            {
                // File input
                return GetFileInput();
            }
            else
            {
                // Invalid choice
                throw new InvalidInputException("Invalid choice. Please type '1', '2', or 'end'.");
            }
        }

        /// <summary>
        /// Reads a Sudoku puzzle string from the console (manual entry).
        /// </summary>
        private string GetManualInput()
        {
            
            Console.WriteLine("Please enter the Sudoku puzzle as a single line (e.g. 81 characters for 9x9).");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new InvalidInputException("Input is empty or contains only whitespace. Please try again.");
            }
            return input;
        }

        /// <summary>
        /// Reads a Sudoku puzzle from a file specified by the user.
        /// </summary>
        private string GetFileInput()
        {
            Console.WriteLine("Please enter the full path to the file containing the Sudoku puzzle (make sure the file contains only one sudoku at a time:");
            string filePath = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new InvalidInputException("File path cannot be empty. Please provide a valid path.");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found. Please check the path and try again.");
            }
            if (!Path.GetExtension(filePath).ToLower().Equals(".txt"))
            {
                throw new InvalidInputException("File must be a .txt file. Please provide a valid .txt file path.");
            }

            try
            {
                
                string fileContent = File.ReadAllText(filePath);

                // trim out extra whitespace/newlines
                fileContent = fileContent.Replace("\r", "").Replace("\n", "").Trim();
                if (string.IsNullOrEmpty(fileContent))
                {
                    throw new InvalidInputException("The file is empty. Please provide a valid Sudoku puzzle.");
                }


                return fileContent;
            }
            catch (IOException ex)
            {
                throw new IOException($"Error reading file: {ex.Message}", ex);

            }
        }
    }
}
