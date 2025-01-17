using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace omegaSudoku.Exceptions
{
    public class SubgridSizeMismatchException : Exception
    {
        public SubgridSizeMismatchException(int size)
            : base($"The board size {size} is invalid. It must be a perfect square.") { }
    }

}
