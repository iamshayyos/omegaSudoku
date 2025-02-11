using omegaSudoku.CoreLogic;
using omegaSudoku.BoardAndCells;
using omegaSudoku.Exceptions;
using Xunit;
using omegaSudoku.Interfaces;


namespace SudokuTests
{
    public class ValidatorTests
    {

        private readonly IValidator _validator;

        public ValidatorTests()
        {
            _validator = new Validator();

        }


        [Fact]
        public void ValidateBoard_ValidBoard_ShouldReturnTrue()
        {
            string validBoard = "400030000000600800000000001000050090080000600070200000000102700503000040900000000";
            SudokuBoard board = new SudokuBoard(validBoard, 9);
            bool isValid = _validator.IsBoardValid(board, 9);
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateBoard_InputTooShort_ShouldThrowException()
        {
            string inValidBoard = "";
            Assert.Throws<InputTooShortException>(() => _validator.IsValidFormat(inValidBoard, out _));
        }

        [Fact]
        public void ValidateBoard_InputTooLarge_ShouldThrowException()
        {
            string largeBoard = new string('1', 700);
            Assert.Throws<InputTooLargeException>(() => _validator.IsValidFormat(largeBoard, out _));
        }

        [Fact]
        public void ValidateBoard_InvalidCharacter_ShouldThrowException()
        {
            string invalidBoard = "123456789X" + new string('1', 72); 
            Assert.Throws<InvalidCharacterException>(() => _validator.IsValidFormat(invalidBoard, out _));
        }

        [Fact]
        public void ValidateBoard_NonPerfectSquare_ShouldThrowException()
        {
            string nonSquareBoard = new string('1', 50); 
            Assert.Throws<InvalidFormatException>(() => _validator.IsValidFormat(nonSquareBoard, out _));
        }
        [Fact]
        public void ValidateBoard_DuplicateNumberInRow_ShouldThrowException()
        {
            // A 9x9 board where the first row has two '1's (which is invalid).
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
            Assert.Throws<BoardInitializationException>(() => _validator.IsBoardValid(board, 9));
        }

    }
}
