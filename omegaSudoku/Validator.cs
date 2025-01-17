using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace omegaSudoku
{
    public class Validator
    {
        private readonly int _maxSize = 25;
        private readonly int _minSize = 4;

        public bool IsValidFormat(string input)
        {
            int boardLen = (int)Math.Sqrt(input.Length);
            return boardLen * boardLen == input.Length && boardLen >= _minSize && boardLen <= _maxSize;
        }

        public bool IsValidInput(string input)
        {
            int boardLen = (int)Math.Sqrt(input.Length);
            foreach (char letter in input)
            {
                if (!IsValidCharacter(letter, boardLen)) return false;
            }
            return true;
        }

        private bool IsValidCharacter(char c, int size)
        {
            if (size <= 9) return c >= '0' && c <= '9';
            if (size <= 16) return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F');
            return false;
        }
    }

}
