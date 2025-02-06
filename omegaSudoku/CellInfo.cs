namespace omegaSudoku
{
    /// <summary>
    /// Helper class to store information about an empty cell:
    /// its position, candidate bit mask, number of candidates, and its degree (influence).
    /// </summary>
    internal class CellInfo
    {
        public int Row { get; }
        public int Col { get; }
        public int CandidateMask { get; }
        public int CandidateCount { get; }
        public int Degree { get; }

        public CellInfo(int row, int col, int candidateMask, int candidateCount, int degree)
        {
            Row = row;
            Col = col;
            CandidateMask = candidateMask;
            CandidateCount = candidateCount;
            Degree = degree;
        }
    }
}
