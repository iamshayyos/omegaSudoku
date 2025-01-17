using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace omegaSudoku.Exceptions
{
    public class BoardInitializationException : Exception
    {
        public BoardInitializationException(string message) : base(message) { }
    }

}
