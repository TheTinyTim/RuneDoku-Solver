using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RuneDoku_Solver
{
    public class RuneDokuSolutionHandler
    {
        // Parent Script
        public Form1 PARENT_SCRIPT;

        // Image Variables
        public Dictionary<int, Bitmap> Runes = new Dictionary<int, Bitmap>();
        public Bitmap RuneDokuBoardBack = (Bitmap)Image.FromFile(Application.StartupPath + "\\Runes\\runedoku_board.bmp");
        public Bitmap runeDokuBoard;

        // Sudoku Variables
        public SudokuBoard sudokuBoard;
        public List<SudokuBoard> solvedSudokuBoard;

        /// <summary>
        /// Construct all the variables needed for this class
        /// </summary>
        /// <param name="parent">The Parent Script Used To Access Other Classes</param>
        /// <returns>The created RuneDokuSolution Class</returns>
        public RuneDokuSolutionHandler ConstructVariables(Form1 parent)
        {
            // set up the parent script
            PARENT_SCRIPT = parent;

            // load all the runes 
            Runes.Add(1, (Bitmap)Image.FromFile(Application.StartupPath + "\\Runes\\mind_rune.bmp"));   // mind rune = 1
            Runes.Add(2, (Bitmap)Image.FromFile(Application.StartupPath + "\\Runes\\fire_rune.bmp"));   // fire rune = 2
            Runes.Add(3, (Bitmap)Image.FromFile(Application.StartupPath + "\\Runes\\body_rune.bmp"));   // body rune = 3
            Runes.Add(4, (Bitmap)Image.FromFile(Application.StartupPath + "\\Runes\\air_rune.bmp"));    // air rune = 4
            Runes.Add(5, (Bitmap)Image.FromFile(Application.StartupPath + "\\Runes\\death_rune.bmp"));  // death rune = 5
            Runes.Add(6, (Bitmap)Image.FromFile(Application.StartupPath + "\\Runes\\water_rune.bmp"));  // water rune = 6
            Runes.Add(7, (Bitmap)Image.FromFile(Application.StartupPath + "\\Runes\\chaos_rune.bmp"));  // chaos rune = 7
            Runes.Add(8, (Bitmap)Image.FromFile(Application.StartupPath + "\\Runes\\earth_rune.bmp"));  // earth rune = 8
            Runes.Add(9, (Bitmap)Image.FromFile(Application.StartupPath + "\\Runes\\law_rune.bmp"));    // law rune = 9

            // now return this script to be stored
            return this;
        }

        /// <summary>
        /// Does the nessecary steps to solve the runedoku board
        /// displayed on the runescape window
        /// </summary>
        /// <param name="RSWindowRect">The Rect For The RSWindow</param>
        /// <returns>The image of the solved Runedoku Board</returns>
        public Bitmap SolveRuneDokuBoard(Rectangle RSWindowRect)
        {
            // capture the runedoku board on the runescape screen
            CaptureRuneDokuBoard(RSWindowRect);
            // go through all the squares of the runedoku board and assign the ones with runes
            // with a numeric value and add it to the sudoku board then solve it
            GenerateSudokuBoard();
            // with the solved sudoku board go through each value and add the appropriate rune
            // to the rundoku board image to be shown
            Bitmap RuneDokuBoard = GeneratSolvedRuneDokuBoard();
            // return the stitched solved runedoku board to be stored and displayed
            return RuneDokuBoard;
        }

        /// <summary>
        /// Stitches together the picture for the solved rundoku board for later use
        /// </summary>
        /// <returns>The image of the solved Runedoku Board</returns>
        public Bitmap GeneratSolvedRuneDokuBoard()
        {
            // make a variable that will store the final result of the sudoku board
            Bitmap solvedRunedokuBoard = RuneDokuBoardBack;
            // store how many rows and columns are in the sudoku board
            Rectangle RuneDokuRect = new Rectangle(0, 0, 9, 9);
            // store the location of the current square being pulled from the board
            Rectangle runeSpot = new Rectangle(7, 4, 32, 32);

            // go through each sudoku square in the previously generated squares and make an image with the runes
            for (int y = 0; y < RuneDokuRect.Height; y++)
            {
                // get the current y location of the square being captured based on the y
                if (y != 0)
                    runeSpot.Y = runeSpot.Y + runeSpot.Height + 5;
                else
                    runeSpot.Y = 4;

                for (int x = 0; x < RuneDokuRect.Width; x++)
                {
                    // get the current x location of the square being captured based on the x
                    if (x != 0)
                        runeSpot.X = runeSpot.X + runeSpot.Width + 5;
                    else
                        runeSpot.X = 7;

                    // get the value of the sudoku square at x/y
                    int rune = solvedSudokuBoard[0].Tile(y, x).Value;
                    // add the rune onto the board in the correct area depending on the x/y of the square
                    solvedRunedokuBoard = AddRuneToBoard(solvedRunedokuBoard, Runes[rune], runeSpot);
                }
            }

            return solvedRunedokuBoard;
        }

        /// <summary>
        /// adds a rune to the solution board in the correct spot
        /// based on the numerical sudoku board
        /// </summary>
        /// <param name="board">The Current Runedoku Board</param>
        /// <param name="rune">The Rune To Be Stitched On The Board</param>
        /// <param name="runeSpot">The Spot Where The Rune Will Be Stitched On The Board</param>
        /// <returns>The Runedoku board with the appropriate rune stitched onto it</returns>
        public Bitmap AddRuneToBoard(Bitmap board, Bitmap rune, Rectangle runeSpot)
        {
            // make a new bitmap based on the board
            Bitmap mergedImage = new Bitmap(343, 334, board.PixelFormat);

            // make the rune transparent
            rune.MakeTransparent(Color.White);

            // merge the rune onto the board in the correct square
            using (Graphics graphics = Graphics.FromImage(mergedImage))
            {
                graphics.DrawImage(board, new Rectangle(0, 0, 343, 334));
                graphics.DrawImage(rune, runeSpot);
            }
            // return the merged image to be stored
            return mergedImage;
        }

        /// <summary>
        /// Captures an image of the runedoku board
        /// that's displayed on the runescape screen
        /// to be stored and used for later
        /// </summary>
        /// <param name="WindowRect"></param>
        public void CaptureRuneDokuBoard(Rectangle WindowRect)
        {
            // make a holder for the runedoku board
            runeDokuBoard = new Bitmap(343, 334);
            // capture the runedoku board on the runescape window
            Graphics graphics = Graphics.FromImage(runeDokuBoard as Image);
            graphics.CopyFromScreen(WindowRect.X + 113, WindowRect.Y + 37, 0, 0, runeDokuBoard.Size);
        }

        /// <summary>
        /// Generates a regulard numerical sudoku board based on the runes
        /// on the runedoku board
        /// </summary>
        public void GenerateSudokuBoard()
        {

            // first lets creat the holder for the sudoku board to be filled out as we gather the runes on the screen
            sudokuBoard = SudokuFactory.ClassicWith3x3Boxes();

            // store how many rows and columns are in the sudoku board
            Rectangle RuneDokuRect = new Rectangle(0, 0, 9, 9);
            // store the location of the current square being pulled from the board
            Rectangle runeSpot = new Rectangle(7, 4, 32, 32);

            // go through each individual square on the runedoku board that was captured and find the squares that have runes in them
            // if they have a rune in the sqaure then find out what kind of rune and add them into the sudoku board to the corrisponding number
            for (int y = 0; y < RuneDokuRect.Height; y++)
            {
                // get the current y location of the square being captured based on the y
                if (y != 0)
                    runeSpot.Y = runeSpot.Y + runeSpot.Height + 5;
                else
                    runeSpot.Y = 4;

                for (int x = 0; x < RuneDokuRect.Width; x++)
                {
                    // get the current x location of the square being captured based on the x
                    if (x != 0)
                        runeSpot.X = runeSpot.X + runeSpot.Width + 5;
                    else
                        runeSpot.X = 7;

                    // set the pixel formating to match the runedoku board
                    System.Drawing.Imaging.PixelFormat format = runeDokuBoard.PixelFormat;
                    // grab the square on the board at x/y
                    Bitmap rune = runeDokuBoard.Clone(runeSpot, format);
                    //rune.Save(Application.StartupPath + "\\Runes\\hello.png");

                    // find out if the square contains a rune or not if it doesn't continue on the loop
                    if (rune.GetPixel(12, 2) == Color.FromArgb(0, 0, 1))
                    {
                        // get the color of the background and make it transparent
                        Color transparencyColor = rune.GetPixel(0, 0);
                        rune.MakeTransparent(transparencyColor);
                        if (y == 8 && (x == 0 || x == 2))
                            rune.MakeTransparent(Color.FromArgb(250, 153, 7));

                        // now its time to find what number this rune is by comparing each rune possible
                        for (int runeNum = 1; runeNum <= 9; runeNum++)
                        {
                            // check to see if the rune is the same as the current rune being cheked
                            if (CompareRunes(rune, Runes[runeNum]))
                            {
                                // if it is the same then add the number to the sudoku row data
                                sudokuBoard.Tile(y, x).Value = runeNum;
                                break;
                            }
                        }
                    }
                }
            }
            // tell the program to solve the board
            solvedSudokuBoard = sudokuBoard.Solve().ToList();
        }

        /// <summary>
        /// copmare 2 runes, one on the board and one preloaded rune
        /// </summary>
        /// <param name="bmp1">Rune On The Board</param>
        /// <param name="bmp2">Preloaded Rune</param>
        /// <returns>If the 2 runes being compared is the same</returns>
        public bool CompareRunes(Bitmap bmp1, Bitmap bmp2)
        {
            // make the preloaded rune transparent
            bmp2.MakeTransparent(Color.White);

            //bmp1.Save(Application.StartupPath + "\\Runes\\Captured_Rune.png");
            //bmp2.Save(Application.StartupPath + "\\Runes\\Compared_Rune.png");
            // check to make sure both runes are the same size
            if (!bmp1.Size.Equals(bmp2.Size))
            {
                return false;
            }
            // go through each pixel and check if they're slightly the same
            for (int x = 0; x < bmp1.Width; ++x)
            {
                for (int y = 0; y < bmp1.Height; ++y)
                {
                    Color bmp1Col = bmp1.GetPixel(x, y);
                    Color bmp2Col = bmp2.GetPixel(x, y);

                    int rDif = Math.Abs(bmp1Col.R - bmp2Col.R);
                    int gDif = Math.Abs(bmp1Col.G - bmp2Col.G);
                    int bDif = Math.Abs(bmp1Col.B - bmp2Col.B);

                    if (rDif > 30 && gDif > 30 && bDif > 30)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
