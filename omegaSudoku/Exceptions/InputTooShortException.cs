using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace omegaSudoku.Exceptions
{
    public class InputTooShortException : Exception
    {
        public InputTooShortException(string message) : base(message) { }

    }
}
