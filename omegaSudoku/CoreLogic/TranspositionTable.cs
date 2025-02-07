using System.Collections.Generic;

namespace omegaSudoku.CoreLogic
{
    /// <summary>
    /// Stores previously seen board configurations to avoid redundant computations.
    /// </summary>
    public class TranspositionTable
    {
        private HashSet<string> _seen;

        public TranspositionTable()
        {
            _seen = new HashSet<string>();
        }

        public bool Contains(string boardHash)
        {
            return _seen.Contains(boardHash);
        }

        public void Add(string boardHash)
        {
            _seen.Add(boardHash);
        }
    }
}
