using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuneDoku_Solver
{
    class OldSudokuSolver
    {

        public class Square
        {
            private readonly List<int> _potentialValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            internal enum Blocks
            {
                UpperLeft,
                UpperMiddle,
                UpperRight,
                MiddleLeft,
                Middle,
                MiddleRight,
                LowerLeft,
                LowerMiddle,
                LowerRight
            }

            public int Row { get; private set; }
            public int Column { get; private set; }

            internal Blocks Block
            {
                get
                {
                    if (Row < 4)
                    {
                        if (Column < 4)
                        {
                            return Blocks.UpperLeft;
                        }

                        return Column < 7 ? Blocks.UpperMiddle : Blocks.UpperRight;
                    }

                    if (Row < 7)
                    {
                        if (Column < 4)
                        {
                            return Blocks.MiddleLeft;
                        }

                        return Column < 7 ? Blocks.Middle : Blocks.MiddleRight;
                    }

                    if (Column < 4)
                    {
                        return Blocks.LowerLeft;
                    }

                    return Column < 7 ? Blocks.LowerMiddle : Blocks.LowerRight;
                }
            }

            public bool IsSolved { get { return Value != null; } }

            public int? Value { get; set; }
            internal List<int> PotentialValues { get; private set; }

            internal Square(int row, int column)
            {
                Row = row;
                Column = column;
                PotentialValues = _potentialValues;
            }
        }

        public class Board
        {
            public List<Square> Squares { get; private set; }

            public Board()
            {
                Squares = new List<Square>();

                for (int row = 1; row < 10; row++)
                {
                    for (int column = 1; column < 10; column++)
                    {
                        Squares.Add(new Square(row, column));
                    }
                }
            }

            public void SetSquareValue(int row, int column, int value)
            {
                Square activeSquare = Squares.Single(x => (x.Row == row) && (x.Column == column));

                activeSquare.Value = value;

                // Remove value from other squares in the same row
                foreach (Square square in Squares.Where(s => !s.IsSolved && (s.Row == row)))
                {
                    square.PotentialValues.Remove(value);
                }

                // Remove value from other squares in the same column
                foreach (Square square in Squares.Where(s => !s.IsSolved && (s.Column == column)))
                {
                    square.PotentialValues.Remove(value);
                }

                // Remove value from other squares in the same quadrant
                foreach (Square square in Squares.Where(s => !s.IsSolved && (s.Block == activeSquare.Block)))
                {
                    square.PotentialValues.Remove(value);
                }

                // Set the Value for any square that only have one remaining PotentialValue
                foreach (Square square in Squares.Where(s => !s.IsSolved && (s.PotentialValues.Count == 1)))
                {
                    SetSquareValue(square.Row, square.Column, square.PotentialValues[0]);
                }
            }
        }

    }
}
