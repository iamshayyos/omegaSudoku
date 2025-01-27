using System;

namespace omegaSudoku
{
    /// <summary>
    /// פותר שמיישם קודם מספר צעדי-היוריסטיקה, ואז רץ עם הבק-טרקינג הרגיל (SudokuSolver).
    /// כמו כן הוסף מנגנון לבחירה אם בכלל להפעיל את ההיוריסטיקות, לפי גודל הלוח ומצבו.
    /// </summary>
    public class SudokuSolverWithHeuristics : ISudokuSolver
    {
        private readonly SudokuSolver _baseSolver = new SudokuSolver();

        public bool Solve(int[,] board, int size)
        {
            // נחליט אם שווה להריץ היוריסטיקות רבות או ישר ללכת לבק־טרקינג
            // לדוגמה: אם הלוח גדול (מעל 16x16) ובעל הרבה תאים ריקים, כדאי להריץ יותר סבבי היוריסטיקה.
            // אם הלוח קטן (9x9) או כמעט מלא, אפשר לגשת ישר לבקטרק (או סבב אחד קצר).

            int emptyCount = CountEmpty(board, size);

            // אם גודל > 16 ויש מעל 100 תאים ריקים, נריץ כמה סבבי-היוריסטיקה
            // אפשר לשחק עם הפרמטרים כרצונכם
            if (size > 16 && emptyCount > 100)
            {
                ApplyHeuristicsRepeatedly(board, size, maxIterations: 10);
            }
            // אם גודל בין 10 ל-16 ויש מעל 50 תאים ריקים - נעשה כמה סבבים
            else if (size >= 10 && emptyCount > 50)
            {
                ApplyHeuristicsRepeatedly(board, size, maxIterations: 8);
            }
            else
            {
                // קטנים יותר או עם קצת תאים ריקים => נעשה כמה סבבים בודדים
                ApplyHeuristicsRepeatedly(board, size, maxIterations: 3);
            }

            //now run the regular backtracking solver
            return _baseSolver.Solve(board, size);
        }

        
        //runs a series of heuristics (currently: naked single + hidden single) until the board stops changing or a max number of iterations is reached
        private void ApplyHeuristicsRepeatedly(int[,] board, int size, int maxIterations)
        {
            bool changed = true;
            int iteration = 0;
            while (changed && iteration < maxIterations)
            {
                iteration++;
                changed = false;

                // Naked Single
                bool singleCandidatesFound = ApplySingleCandidate(board, size);
                if (singleCandidatesFound) changed = true;

                // Hidden Single
                bool hiddenSinglesFound = ApplyHiddenSingle(board, size);
                if (hiddenSinglesFound) changed = true;
            }
        }

        
        //if there is only one possible value for a cell, place it there
        private bool ApplySingleCandidate(int[,] board, int size)
        {
            bool foundChange = false;

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (board[r, c] == 0)
                    {
                        int candidateCount = 0;
                        int candidateValue = 0;

                        for (int val = 1; val <= size; val++)
                        {
                            if (IsSafe(board, r, c, val, size))
                            {
                                candidateCount++;
                                if (candidateCount > 1) break;
                                candidateValue = val;
                            }
                        }

                        if (candidateCount == 1)
                        {
                            board[r, c] = candidateValue;
                            foundChange = true;
                        }
                    }
                }
            }

            return foundChange;
        }

        
        // if value is only possible in one spot in a row, column, or box, place it there
        private bool ApplyHiddenSingle(int[,] board, int size)
        {
            bool changed = false;
            int subSize = (int)Math.Sqrt(size);

            //check in each row
            for (int row = 0; row < size; row++)
            {
                for (int val = 1; val <= size; val++)
                {
                    int possibleSpotCount = 0;
                    int lastSpotCol = -1;

                    for (int c = 0; c < size; c++)
                    {
                        if (board[row, c] == 0 && IsSafe(board, row, c, val, size))
                        {
                            possibleSpotCount++;
                            if (possibleSpotCount > 1) break;
                            lastSpotCol = c;
                        }
                    }

                    if (possibleSpotCount == 1 && board[row, lastSpotCol] == 0)
                    {
                        board[row, lastSpotCol] = val;
                        changed = true;
                    }
                }
            }

            //check in each column
            for (int col = 0; col < size; col++)
            {
                for (int val = 1; val <= size; val++)
                {
                    int possibleSpotCount = 0;
                    int lastSpotRow = -1;

                    for (int r = 0; r < size; r++)
                    {
                        if (board[r, col] == 0 && IsSafe(board, r, col, val, size))
                        {
                            possibleSpotCount++;
                            if (possibleSpotCount > 1) break;
                            lastSpotRow = r;
                        }
                    }

                    if (possibleSpotCount == 1 && board[lastSpotRow, col] == 0)
                    {
                        board[lastSpotRow, col] = val;
                        changed = true;
                    }
                }
            }

            //check in each box
            for (int boxRow = 0; boxRow < size; boxRow += subSize)
            {
                for (int boxCol = 0; boxCol < size; boxCol += subSize)
                {
                    for (int val = 1; val <= size; val++)
                    {
                        int possibleSpotCount = 0;
                        int lastSpotR = -1;
                        int lastSpotC = -1;

                        for (int r = 0; r < subSize; r++)
                        {
                            for (int c = 0; c < subSize; c++)
                            {
                                int rr = boxRow + r;
                                int cc = boxCol + c;
                                if (board[rr, cc] == 0 && IsSafe(board, rr, cc, val, size))
                                {
                                    possibleSpotCount++;
                                    if (possibleSpotCount > 1) goto NextBoxVal;
                                    lastSpotR = rr;
                                    lastSpotC = cc;
                                }
                            }
                        }
                    NextBoxVal:;
                        if (possibleSpotCount == 1 && board[lastSpotR, lastSpotC] == 0)
                        {
                            board[lastSpotR, lastSpotC] = val;
                            changed = true;
                        }
                    }
                }
            }

            return changed;
        }

        private bool IsSafe(int[,] board, int row, int col, int val, int size)
        {
            //cehck row and column
            for (int i = 0; i < size; i++)
            {
                if (board[row, i] == val) return false;
                if (board[i, col] == val) return false;
            }

            //check subgrid
            int subSize = (int)Math.Sqrt(size);
            int startRow = (row / subSize) * subSize;
            int startCol = (col / subSize) * subSize;

            for (int rr = 0; rr < subSize; rr++)
            {
                for (int cc = 0; cc < subSize; cc++)
                {
                    if (board[startRow + rr, startCol + cc] == val)
                        return false;
                }
            }

            return true;
        }

        private int CountEmpty(int[,] board, int size)
        {
            int count = 0;
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (board[r, c] == 0) count++;
                }
            }
            return count;
        }
    }
}

