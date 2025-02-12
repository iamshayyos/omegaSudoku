using omegaSudoku.Board;
using omegaSudoku.CoreLogic;
using omegaSudoku.Exceptions;
using omegaSudoku.Heuristics;
using omegaSudoku.Interfaces;
using System.Collections.Generic;

public class FastAdvanced25x25Solver : ISudokuSolver
{
    // Interval for performing global constraint checks
    private const int CHECK_INTERVAL = 10; 
    private readonly List<IHeuristic> _heuristics;

    public FastAdvanced25x25Solver()
    {
        // Initialize the heuristics list dynamically
        _heuristics = new List<IHeuristic>
        {
            new NakedSinglesHeuristic(),
            new HiddenSingleHeuristic(),
            new NakedPairsHeuristic(),
        };
    }

    public bool Solve(SudokuBoard sudokuBoard)
    {
        var state = new SolverState(sudokuBoard);

        if (state.Size >= 16)
        {
            bool changed;
            do
            {
                changed = false;
                foreach (var heuristic in _heuristics)
                {
                    if (heuristic.Apply(sudokuBoard, state))
                        changed = true;
                }
            } while (changed);
        }

        // MRV Backtracking with periodic global constraint checks.
        var globalChecker = new GlobalConstraintChecker();
        var mrvSolver = new MRVBacktrackingSolver(globalChecker, CHECK_INTERVAL);

        if (!mrvSolver.Solve(state))
        {
            throw new UnsolvableBoardException("Board cannot be solved.");
        }
        return true;
    }
}
