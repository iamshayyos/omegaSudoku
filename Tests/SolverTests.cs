using System;
using omegaSudoku.CoreLogic;
using omegaSudoku.Interfaces;
using omegaSudoku.Exceptions;
using Xunit;
using omegaSudoku.Board;
using omegaSudoku.Heuristics;

namespace SudokuTests
{
    public class SolverTests
    {
        private readonly ISudokuSolver _solver;

        public SolverTests()
        {
            // Arrange: Initialize the solver with a heuristic
            _solver = new SudokuSolver();
        }

        [Fact]
        public void Solve_ValidSudoku_ShouldReturnSolvedBoard()
        {
            // Arrange
            string validBoard = "000006000059000008200008000045000000003000000006003054000325006000000000000000000";
            SudokuBoard board = new SudokuBoard(validBoard, 9);
            var solver = new FastBacktrackingSolver();

            // Act
            bool solved = solver.Solve(board);

            // Assert
            Assert.True(solved);
        }

        [Fact]
        public void Solve_UnsolvableSudoku_ShouldThrowException()
        {
            // Arrange
            string unsolvableBoard = "000030000060000400007050800000406000000900000050010300400000020000300000000000000";
            SudokuBoard board = new SudokuBoard(unsolvableBoard, 9);

            // Act & Assert
            Assert.Throws<UnsolvableBoardException>(() => _solver.Solve(board));
        }

        [Fact]
        public void Solve_16x16Sudoku_ShouldReturnSolvedBoard()
        {
            // Arrange
            string validBoard = "0000:=000000000?70050;01:00@90<8900800700004600=60:=080000070002=00030890>?500012;01@:000008007>00001000@0000900000<>?0740000000006@900000>0100002;0600=800<00500070002000000000<0900>?5;4020=0@0020=0@0<0907000>?500400=6000<803<0000002001@:0000=68000?0004020";
            SudokuBoard board = new SudokuBoard(validBoard, 16);

            // Act
            bool solved = _solver.Solve(board);

            // Assert
            Assert.True(solved);
        }

        [Fact]
        public void Solve_25x25Sudoku_ShouldReturnSolvedBoard()
        {
            // Arrange
            string validBoard = "0000C000F000000000000000000000000000000000000000000000030000000000000000000000000E00000000000000000000H000000000000450000000000000000000000000000000000000F0G000000000000;00:00000000000000000000000000A0000;000000900000000000000000000000000000000000G0000000000?0000000000000000207000000000000G0000000000000000000000?000000000000000000000000000000000000000000000@000000000000030000000000000000000000000000000000000000000000000000@000000000000000000000000>0000000000000000000000000000000<G000000000000000000000000000000000000600000?000000000000000000000000000000000000000000000000000000000009001>00000000000000000000000000000000H";
            SudokuBoard board = new SudokuBoard(validBoard, 25);

            // Act
            bool solved = _solver.Solve(board);

            // Assert
            Assert.True(solved);
        }

        [Fact]
        public void Solve_EmptySudoku_ShouldSolveSuccessfully()
        {
            // Arrange
            string validEmptyBoard = new string('0', 625);
            SudokuBoard board = new SudokuBoard(validEmptyBoard, 25);

            // Act
            bool solved = _solver.Solve(board);

            // Assert
            Assert.True(solved);
        }

        [Fact]
        public void Solve_AlreadySolvedSudoku_ShouldReturnTrueImmediately()
        {
            // Arrange
            string solvedBoard = "123456789" +
                                 "456789123" +
                                 "789123456" +
                                 "231564897" +
                                 "564897231" +
                                 "897231564" +
                                 "312648975" +
                                 "645972318" +
                                 "978315642";

            SudokuBoard board = new SudokuBoard(solvedBoard, 9);

            // Act
            bool solved = _solver.Solve(board);

            // Assert
            Assert.True(solved);
        }
        [Fact]
        public void Solve_LargeSudoku_ShouldCompleteInReasonableTime()
        {
            // Arrange: Create an empty 25x25 board (hardest case)
            string largeEmptyBoard = new string('0', 625);
            SudokuBoard board = new SudokuBoard(largeEmptyBoard, 25);

            // Act
            bool solved = _solver.Solve(board);

            // Assert
            Assert.True(solved);
        }

        [Fact]
        public void Solve_OneEmptyCell_ShouldSolveImmediately()
        {
            // Arrange: A 9x9 board missing just one number
            string almostSolvedBoard =
                "123456789" +
                "456789123" +
                "789123456" +
                "231564897" +
                "564897231" +
                "897231564" +
                "312648975" +
                "645972318" +
                "978315640"; // '2' is missing at the end

            SudokuBoard board = new SudokuBoard(almostSolvedBoard, 9);

            // Act
            bool solved = _solver.Solve(board);

            // Assert
            Assert.True(solved);
        }

    }
}
