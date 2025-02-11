using System;
namespace omegaSudoku.Board
{
    public class SudokuBoard
    {
        public int[,] Board { get; private set; }
        public int Size { get; private set; }
        // Constructor
        public SudokuBoard(string input, int size)
        {
            Size = size;
            Board = new int[size, size];
            InitializeBoard(input);
        }
        // Initialize the board
        private void InitializeBoard(string input)
        {
            for (int i = 0; i < input.Length && i < Size * Size; i++)
            {
                Board[i / Size, i % Size] = CharToInt(input[i]);
            }
        }
        // Convert char to int
        private int CharToInt(char c)
        {
            if (char.IsDigit(c))
                return c - '0';
            if (char.IsLetter(c))
                return char.ToUpper(c) - 'A' + 10;
            return 0;
        }
        // Convert cell value to string
        private string CellValueToString(int val)
        {
            return val == 0 ? "." : val.ToString();
        }
        // Print the board
        public void PrintBoard()
        {
            int subSize = (int)Math.Sqrt(Size);

            int maxLen = 1;
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    string cellStr = CellValueToString(Board[r, c]);
                    if (cellStr.Length > maxLen)
                        maxLen = cellStr.Length;
                }
            }
            int cellWidth = maxLen + 1;

            string CreateHorizontalLine()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int col = 0; col < Size; col++)
                {
                    if (col % subSize == 0)
                    {
                        sb.Append('+');
                    }
                    sb.Append(new string('-', cellWidth));
                }
                sb.Append('+');
                return sb.ToString();
            }

            for (int row = 0; row < Size; row++)
            {
                if (row % subSize == 0)
                {
                    Console.WriteLine(CreateHorizontalLine());
                }

                for (int col = 0; col < Size; col++)
                {
                    if (col % subSize == 0)
                    {
                        Console.Write("|");
                    }

                    string valStr = CellValueToString(Board[row, col]);
                    Console.Write(valStr.PadLeft(cellWidth));
                }
                Console.WriteLine("|");
            }

            Console.WriteLine(CreateHorizontalLine());
        }
    }
}