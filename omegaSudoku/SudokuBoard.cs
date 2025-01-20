using System;

namespace omegaSudoku
{
    public class SudokuBoard
    {
        public int[,] Board { get; private set; }
        public int Size { get; private set; }

        public SudokuBoard(string input, int size)
        {
            Size = size;
            Board = new int[size, size];
            InitializeBoard(input);
        }

        private void InitializeBoard(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                Board[i / Size, i % Size] = CharToInt(input[i]);
            }
        }

        private int CharToInt(char c)
        {
            if (char.IsDigit(c)) return c - '0';
            if (char.IsLetter(c)) return char.ToUpper(c) - 'A' + 10;
            return 0;
        }

        public void PrintBoard()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (j % Math.Sqrt(Size) == 0 && j != 0) Console.Write("| ");
                    Console.Write(Board[i, j] == 0 ? ". " : Board[i, j] + " ");
                }
                Console.WriteLine();
                if ((i + 1) % Math.Sqrt(Size) == 0 && i != Size - 1)
                {
                    Console.WriteLine(new string('-', Size * 2 + (int)Math.Sqrt(Size) - 1));
                }
            }
        }
    }
}
