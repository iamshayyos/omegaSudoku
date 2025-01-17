using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    Console.Write(Board[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
    }

}
