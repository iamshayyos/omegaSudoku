using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace omegaSudoku
{
    public class InputHandler
    {
        public string GetInput()
        {
            Console.WriteLine("Enter the Sudoku board:");
            return Console.ReadLine();
        }
    }

    public class OutputHandler
    {
        public void PrintMessage(string message)
        {
            Console.WriteLine(message);
        }
    }

}
