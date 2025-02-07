using System;

namespace omegaSudoku.BoardAndCells
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
            if (char.IsDigit(c))
                return c - '0';
            if (char.IsLetter(c))
                return char.ToUpper(c) - 'A' + 10;
            return 0;
        }

        public void PrintBoard()
        {
            int subSize = (int)Math.Sqrt(Size);
            for (int i = 0; i < Size; i++)
            {
                if (i > 0 && i % subSize == 0)
                {
                    Console.WriteLine(new string('-', Size * 3 / 2));
                }
                for (int j = 0; j < Size; j++)
                {
                    if (j > 0 && j % subSize == 0)
                        Console.Write("| ");
                    int val = Board[i, j];
                    Console.Write(val == 0 ? ". " : val + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
