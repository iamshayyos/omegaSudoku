using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace omegaSudoku.Exceptions
{
    public class InputTooLargeException : Exception
    {
        public InputTooLargeException(string message) : base(message) { }
    }

}
