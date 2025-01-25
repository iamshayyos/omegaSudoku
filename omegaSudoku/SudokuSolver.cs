using System;

namespace omegaSudoku
{
    public class SudokuSolver
    {
        private int[,] _board;          
        private int _size;              
        private int _subSize;           
        private int _fullMask;          

        private int[,] _candidateMasks;

        private int[,] _candidateCount;

        
        public bool Solve(int[,] board, int size)
        {
            _size = size;
            _subSize = (int)Math.Sqrt(size);
            _fullMask = (1 << size) - 1; 
            _board = new int[size, size];

            _candidateMasks = new int[size, size];
            _candidateCount = new int[size, size];

            Array.Copy(board, _board, board.Length);

            InitializeCandidates();

            if (!ApplyInitialValues())
            {
                return false;
            }

            return Backtrack();
        }

       
        private void InitializeCandidates()
        {
            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    _candidateMasks[r, c] = _fullMask;    
                    _candidateCount[r, c] = _size;        
                }
            }
        }

        
        private bool ApplyInitialValues()
        {
            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    int val = _board[r, c];
                    if (val != 0)
                    {
                        if (!PlaceValue(r, c, val))
                        {
                            return false; 
                        }
                    }
                }
            }
            return true;
        }

     
        private bool Backtrack()
        {
            int bestR = -1, bestC = -1;
            int minCount = int.MaxValue;

            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    if (_board[r, c] == 0) 
                    {
                        int count = _candidateCount[r, c];
                        if (count < minCount)
                        {
                            minCount = count;
                            bestR = r;
                            bestC = c;
                            if (minCount <= 1) 
                                break;
                        }
                    }
                }
                if (minCount <= 1) break;
            }

            if (bestR == -1) return true;

            if (minCount == 0) return false;

            int mask = _candidateMasks[bestR, bestC];
            for (int val = 1; val <= _size; val++)
            {
                int bit = 1 << (val - 1);
                if ((mask & bit) != 0)
                {
                    if (PlaceValue(bestR, bestC, val))
                    {
                        if (Backtrack())
                            return true;

                        RemoveValue(bestR, bestC, val);
                    }
                }
            }

            return false;
        }

       
        private bool PlaceValue(int r, int c, int val)
        {
            int bit = 1 << (val - 1);

            if ((_candidateMasks[r, c] & bit) == 0)
                return false;

            _board[r, c] = val;

            _candidateMasks[r, c] = 0;
            _candidateCount[r, c] = 0;

            for (int col = 0; col < _size; col++)
            {
                if (col != c && _board[r, col] == 0)
                {
                    if (!RemoveCandidate(r, col, bit))
                        return false;
                }
            }

            for (int row = 0; row < _size; row++)
            {
                if (row != r && _board[row, c] == 0)
                {
                    if (!RemoveCandidate(row, c, bit))
                        return false;
                }
            }

            int subRow = (r / _subSize) * _subSize;
            int subCol = (c / _subSize) * _subSize;
            for (int rr = 0; rr < _subSize; rr++)
            {
                for (int cc = 0; cc < _subSize; cc++)
                {
                    int nr = subRow + rr;
                    int nc = subCol + cc;
                    if ((nr != r || nc != c) && _board[nr, nc] == 0)
                    {
                        if (!RemoveCandidate(nr, nc, bit))
                            return false;
                    }
                }
            }

            return true;
        }

      
        private void RemoveValue(int r, int c, int val)
        {
            _board[r, c] = 0;          

            int bit = 1 << (val - 1);
            _candidateMasks[r, c] |= bit;
            _candidateCount[r, c] = PopCount(_candidateMasks[r, c]);

            for (int col = 0; col < _size; col++)
            {
                if (col != c && _board[r, col] == 0)
                {
                    RestoreCandidate(r, col, bit);
                }
            }

            for (int row = 0; row < _size; row++)
            {
                if (row != r && _board[row, c] == 0)
                {
                    RestoreCandidate(row, c, bit);
                }
            }

            int subRow = (r / _subSize) * _subSize;
            int subCol = (c / _subSize) * _subSize;
            for (int rr = 0; rr < _subSize; rr++)
            {
                for (int cc = 0; cc < _subSize; cc++)
                {
                    int nr = subRow + rr;
                    int nc = subCol + cc;
                    if ((nr != r || nc != c) && _board[nr, nc] == 0)
                    {
                        RestoreCandidate(nr, nc, bit);
                    }
                }
            }
        }

        private bool RemoveCandidate(int row, int col, int bit)
        {
            int oldMask = _candidateMasks[row, col];
            if ((oldMask & bit) != 0)
            {
                int newMask = oldMask & ~bit;  
                _candidateMasks[row, col] = newMask;
                int newCount = _candidateCount[row, col] - 1;
                _candidateCount[row, col] = newCount;

                if (newCount == 0)
                {
                    return false;
                }
            }
            return true;
        }

  
        private void RestoreCandidate(int row, int col, int bit)
        {
            int oldMask = _candidateMasks[row, col];
            if ((oldMask & bit) == 0)
            {
                int newMask = oldMask | bit;
                _candidateMasks[row, col] = newMask;
                _candidateCount[row, col] = PopCount(newMask);
            }
        }

        private int PopCount(int mask)
        {
            int count = 0;
            while (mask != 0)
            {
                mask &= (mask - 1);
                count++;
            }
            return count;
        }
    }
}
