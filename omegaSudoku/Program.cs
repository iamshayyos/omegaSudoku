using omegaSudoku;
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
