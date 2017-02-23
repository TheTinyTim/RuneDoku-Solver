using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RuneDoku_Solver
{
    public class WindowHandler
    {
        // Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        // DLLImports
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out Rectangle rectangle);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        // Rectangle Variables
        public List<Rectangle> buttonList = new List<Rectangle>();
        public Rectangle LastBounds;

        // Form Variables
        public Form SolveButtonForm;
        public Form runeDokuWindow;

        // Bool Variables
        public bool close = false;

        // Image Variables
        public Bitmap SolveButton = (Bitmap)Image.FromFile(Application.StartupPath + "\\Runes\\solve_button.bmp");

        // Parent Script
        public Form1 PARENT_SCRIPT;

        /// <summary>
        /// Construct all the variables needed for this class
        /// </summary>
        /// <param name="parent">The Parent Script Used To Access Other Classes</param>
        /// <returns>The created WindowHandler Class</returns>
        public WindowHandler ConstructVariables(Form1 parent)
        {
            // set up the parent script for later use
            PARENT_SCRIPT = parent;
          
            // store empty forms in both the solve button and runedoku variables to not raise a null error
            SolveButtonForm = new Form();
            runeDokuWindow = new Form();
            // and then dispose them
            SolveButtonForm.Dispose();
            runeDokuWindow.Dispose();

            // now return this script back to be stored
            return this;
        }

        public void UpdateButtons()
        {
            // get the rect of the rs window
            Rectangle RSWindowRect = GetRSWindowRect();
            // clear the buttons
            buttonList.Clear();
            // solve button
            buttonList.Add(new Rectangle(RSWindowRect.X + 471, RSWindowRect.Y + 91, 42, 24));
            // open casket button
            buttonList.Add(new Rectangle(RSWindowRect.X + 14, RSWindowRect.Y + 91, 84, 24));
            // close button
            buttonList.Add(new Rectangle(RSWindowRect.X + 12, RSWindowRect.Y + 43, 24, 23));
        }

        /// <summary>
        /// Finds the rect of the runescape window and returns it
        /// </summary>
        /// <returns>The rect of the Runescape Window</returns>
        public Rectangle GetRSWindowRect()
        {
            // get the rect of the runescape window
            Rectangle RSWindowRect;
            if (!GetWindowRect(PARENT_SCRIPT.RSWindowHandle, out RSWindowRect))
            {
                return new Rectangle();
            }
            return RSWindowRect;
        }

        /// <summary>
        /// Constantly check if the Runedoku minigame window is open
        /// </summary>
        public void RunedokuWindowChecker()
        {
            // update the buttons
            UpdateButtons();

            IntPtr rsWindowHandle = GetForegroundWindow();
            if (rsWindowHandle == PARENT_SCRIPT.RSWindowHandle)
            {
                // get the rect of the runescape window
                Rectangle RSWindowRect = GetRSWindowRect();
                // grab a pixel of the board that will be unique to anything else
                if (CheckScreenPixel(new Point(489,320),Color.FromArgb(98,44,12), RSWindowRect))
                {
                    // make a new form to hold the runedoku board
                    SolveButtonForm = FormMaker("Solve Button", SolveButton, 0.99d, buttonList[0]);
                    // display the solve button
                    SolveButtonForm.Show();
                    // set the last known bounds of the rswindow
                    LastBounds = RSWindowRect;
                }
            }
        }

        /// <summary>
        /// Display the Solve button as well as check if the user has pressed any of the buttons
        /// </summary>
        public void RunedokuWindowButtonHandler()
        {
            // update the buttons position
            UpdateButtons();
            // get the position of the mouse and store it to be used for later
            POINT mousePos;
            GetCursorPos(out mousePos);
            // get the rect of the rswindow
            Rectangle RSWindowRect = GetRSWindowRect();

            if (buttonList[0].Contains(mousePos) && PARENT_SCRIPT.HOOK_HANDLER.lmbClicked && runeDokuWindow.IsDisposed)
            {
                Bitmap solvedRunedokuBoard = PARENT_SCRIPT.RUNEDOKU_SOLUTION.SolveRuneDokuBoard(RSWindowRect);
                DrawRuneDokuBoardSolution(solvedRunedokuBoard, RSWindowRect);
                LastBounds = RSWindowRect;
                PARENT_SCRIPT.HOOK_HANDLER.lmbClicked = false;
            }

            if (PARENT_SCRIPT.HOOK_HANDLER.lmbClicked && buttonList[2].Contains(mousePos) || PARENT_SCRIPT.HOOK_HANDLER.escPressed)
            {
                PARENT_SCRIPT.HOOK_HANDLER.lmbClicked = false;
                PARENT_SCRIPT.HOOK_HANDLER.escPressed = false;
                close = true;
            }

            if (PARENT_SCRIPT.HOOK_HANDLER.lmbClicked && (buttonList[1].Contains(mousePos)))
            {
                PARENT_SCRIPT.HOOK_HANDLER.lmbClicked = false;
                close = true;
            }
        }

        /// <summary>
        /// constantly update the form that is being passed through the function
        /// </summary>
        /// <param name="form">The form needing to be updated</param>
        /// <param name="formRect">the relative position the form needs to be ontop of the runescape window</param>
        public void UpdateForm(Form form, Rectangle formRect)
        {
            // get the rect of the runescape window
            Rectangle RSWindowRect = GetRSWindowRect();
            IntPtr rsWindowHandle = PARENT_SCRIPT.RSWindowHandle;

            // check to make sure the user is still on the same window it was activated on
            if (rsWindowHandle == PARENT_SCRIPT.RSWindowHandle)
            {
                // check to see if the window is the topmost window if not make it
                if (!form.TopMost)
                    form.TopMost = true;
                // only update the location if it has moved
                if (RSWindowRect != LastBounds)
                {
                    // if so update the window to always stay in position
                    form.Bounds = new Rectangle(RSWindowRect.X + formRect.X, RSWindowRect.Y + formRect.Y, formRect.Width, formRect.Height);
                    LastBounds = RSWindowRect;
                }
            }
            else
            {
                // if not make sure the window stays where it's at and make it not show always on top
                if (form.TopMost)
                    form.TopMost = false;
            }
        }

        /// <summary>
        /// Makes and displays the solved runedoku board ontop of the runescape screen
        /// </summary>
        /// <param name="solvedRunedokuBoard">The solved image if the runedoku board</param>
        /// <param name="WindowRect">The rect of the runscape window</param>
        public void DrawRuneDokuBoardSolution(Bitmap solvedRunedokuBoard, Rectangle WindowRect)
        {
            // make a new form to hold the runedoku board
            runeDokuWindow = FormMaker("RuneDoku Board",solvedRunedokuBoard,0.60d,new Rectangle(WindowRect.X + 113, WindowRect.Y + 37, 343, 334));
            // show the board
            runeDokuWindow.Show();
        }

        /// <summary>
        /// Creates a form based on the passed variables
        /// </summary>
        /// <param name="formName">The name of the form created</param>
        /// <param name="formBackground">The background image for the form</param>
        /// <param name="formOpacity">The opacity for the form</param>
        /// <param name="formRect">The relative position of the runescape window</param>
        /// <returns></returns>
        public Form FormMaker(string formName, Bitmap formBackground, double formOpacity, Rectangle formRect)
        {
            // make the temp form
            Form tempForm = new Form();
            // set the name of the form
            tempForm.Name = formName;
            // remove the border of the form
            tempForm.FormBorderStyle = FormBorderStyle.None;
            // make the start position of the form what the rect is
            tempForm.StartPosition = FormStartPosition.Manual;
            // add the background of the form
            tempForm.BackgroundImage = formBackground;
            // set the opacity for the form
            tempForm.Opacity = formOpacity;
            // set the form to be above everything else
            tempForm.TopMost = true;
            // set the new form window to be able to click through
            int initialStyle = GetWindowLong(tempForm.Handle, -20);
            SetWindowLong(tempForm.Handle, -20, initialStyle | 0x80000 | 0x20);
            // set the position of the form
            tempForm.Bounds = formRect;
            // return the temp form to be stored
            return tempForm;
        }

        public bool CheckScreenPixel(Point point, Color pixelColorCheck, Rectangle RSWindowRect)
        {
            try
            {
                //Bitmap RSScreen = new Bitmap(RSWindowRect.Width - 45, RSWindowRect.Height - 103);
                Bitmap RSScreen = new Bitmap(1, 1);
                Graphics graphics = Graphics.FromImage(RSScreen as Image);
                graphics.CopyFromScreen(RSWindowRect.X + point.X, RSWindowRect.Y + point.Y, 0, 0, new Size(1, 1));

                Color RSScreenPixel = RSScreen.GetPixel(0, 0);
                int rDiff = Math.Abs(RSScreenPixel.R - pixelColorCheck.R);
                int gDiff = Math.Abs(RSScreenPixel.G - pixelColorCheck.G);
                int bDiff = Math.Abs(RSScreenPixel.B - pixelColorCheck.B);
                // if the pixel grabbed is close show the solve button and run the code
                // to check if the buttons get pressed
                if (rDiff <= 10 && gDiff <= 10 && bDiff <= 10)
                {
                    RSScreen.Dispose();
                    return true;
                }
                RSScreen.Dispose();
                return false;
            }
            catch (Exception e) { return false; }
        }
    }
}
