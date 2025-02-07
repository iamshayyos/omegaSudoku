using System;
using System.IO;

namespace omegaSudoku.IO
{
    /// <summary>
    /// Handles the logic for retrieving a Sudoku puzzle string 
    /// from either manual (console) input or from a file.
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
            Console.WriteLine("2) Read Sudoku puzzle from a file");
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
                Console.WriteLine("Invalid choice. Please type '1', '2', or 'end'.");
                return null; 
            }
        }

        /// <summary>
        /// Reads a Sudoku puzzle string from the console (manual entry).
        /// </summary>
        private string GetManualInput()
        {
            Console.WriteLine("Please enter the Sudoku puzzle as a single line (e.g. 81 characters for 9x9).");
            string input = Console.ReadLine();

            
            return input;
        }

        /// <summary>
        /// Reads a Sudoku puzzle from a file specified by the user.
        /// </summary>
        private string GetFileInput()
        {
            Console.WriteLine("Please enter the full path to the file containing the Sudoku puzzle:");
            string filePath = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("File path cannot be empty.");
                return null;
            }

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found. Please check the path and try again.");
                return null;
            }

            try
            {
                
                string fileContent = File.ReadAllText(filePath);

                // trim out extra whitespace/newlines
                fileContent = fileContent.Replace("\r", "")
                                         .Replace("\n", "")
                                         .Trim();

                
                return fileContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return null;
            }
        }
    }
}
