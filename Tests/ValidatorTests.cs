using omegaSudoku.CoreLogic;
using omegaSudoku.Exceptions;
using Xunit;
using omegaSudoku.Interfaces;
using omegaSudoku.Board;

namespace SudokuTests
{
    public class ValidatorTests
    {
        private readonly IValidator _validator;

        public ValidatorTests()
        {
            // Arrange: Initialize the validator
            _validator = new Validator();
        }

        [Fact]
        public void ValidateBoard_ValidBoard_ShouldReturnTrue()
        {
            // Arrange
            string validBoard = "400030000000600800000000001000050090080000600070200000000102700503000040900000000";
            SudokuBoard board = new SudokuBoard(validBoard, 9);

            // Act
            bool isValid = _validator.IsBoardValid(board, 9);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateBoard_InputTooShort_ShouldThrowException()
        {
            // Arrange
            string invalidBoard = "";

            // Act & Assert
            Assert.Throws<InputTooShortException>(() => _validator.IsValidFormat(invalidBoard, out _));
        }

        [Fact]
        public void ValidateBoard_InputTooLarge_ShouldThrowException()
        {
            // Arrange
            string largeBoard = new string('1', 700);

            // Act & Assert
            Assert.Throws<InputTooLargeException>(() => _validator.IsValidFormat(largeBoard, out _));
        }

        [Fact]
        public void ValidateBoard_InvalidCharacter_ShouldThrowException()
        {
            // Arrange
            string invalidBoard = "123456789X" + new string('1', 72);

            // Act & Assert
            Assert.Throws<InvalidCharacterException>(() => _validator.IsValidFormat(invalidBoard, out _));
        }

        [Fact]
        public void ValidateBoard_NonPerfectSquare_ShouldThrowException()
        {
            // Arrange
            string nonSquareBoard = new string('1', 50);

            // Act & Assert
            Assert.Throws<InvalidFormatException>(() => _validator.IsValidFormat(nonSquareBoard, out _));
        }

        [Fact]
        public void ValidateBoard_DuplicateNumberInRow_ShouldThrowException()
        {
            // Arrange: A 9x9 board with a duplicate '1' in the first row (invalid)
            string invalidRowBoard =
                "100000000" +
                "120000000" +
                "003000000" +
                "000400000" +
                "000050000" +
                "000006000" +
                "000000700" +
                "000000080" +
                "000000009";
            SudokuBoard board = new SudokuBoard(invalidRowBoard, 9);

            // Act & Assert
            Assert.Throws<BoardInitializationException>(() => _validator.IsBoardValid(board, 9));
        }
    }
}
