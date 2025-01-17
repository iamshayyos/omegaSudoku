using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace omegaSudoku.Exceptions
{
    public class InvalidCharacterException : Exception
    {
        public InvalidCharacterException(char invalidChar)
            : base($"Invalid character '{invalidChar}' found in the input.") { }
    }

}
