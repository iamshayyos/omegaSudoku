using System;

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
        for (int i = 0; i < input.Length && i < Size * Size; i++)
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

    private string CellValueToString(int val)
    {
        return val == 0 ? "." : val.ToString();
    }

    public void PrintBoard()
    {
        int subSize = (int)Math.Sqrt(Size);

        // Determine the maximum cell width for alignment
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

        // Create a horizontal line
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

        // Print rows
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

        // Print bottom border
        Console.WriteLine(CreateHorizontalLine());
    }
}
