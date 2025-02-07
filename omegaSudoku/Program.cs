using omegaSudoku.Interfaces;
using omegaSudoku.CoreLogic;
using omegaSudoku.IO;
using omegaSudoku.Heuristics;
using System;

namespace OmegaSudoku
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IValidator validator = new Validator();
            ISudokuSolver solver = new SudokuSolver(new IHeuristic[]
            {
                new SingleCandidateHeuristic(),
                new HiddenSingleHeuristic()
            });

            IOHandler ioHandler = new IOHandler(solver, validator);
            ioHandler.Run();
        }
    }
}
