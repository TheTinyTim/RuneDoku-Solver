using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace RuneDoku_Solver
{
    public partial class Form1 : Form
    {
        // Enums
        public enum SudokuProgress { FAILED, NO_PROGRESS, PROGRESS }

        // DLLImports
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(String sClassName, String sAppName);        
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        
        // Timers
        Timer ProgramLoop;

        // Stopwatch
        Stopwatch stopwatch = new Stopwatch();
  
        // Classes
        public WindowHandler WINDOW_HANDLER;
        public HookHandler HOOK_HANDLER;
        public RuneDokuSolution RUNEDOKU_SOLUTION;

        // IntPtr
        public IntPtr RSWindowHandle = IntPtr.Zero;
           
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load all the variables needed for this program
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            // set the window state so it will go into the taskbar and not the applicaiton bar
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(20);
            Application.EnableVisualStyles();

            // create the context menu for the notify icon to have an exit button
            ContextMenu contextMenu = new ContextMenu();
            MenuItem helpMenuItem = new MenuItem();
            MenuItem exitMenuItem = new MenuItem();
            contextMenu.MenuItems.Add(helpMenuItem);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(exitMenuItem);
            // tie the help window to open with the correct menu item
            helpMenuItem.Index = 0;
            helpMenuItem.Text = "Help";
            // tie the function to close the program with the correct menu item
            exitMenuItem.Index = 2;
            exitMenuItem.Text = "Exit";
            exitMenuItem.Click += new System.EventHandler(CloseProgram);
            // add the context menu to the notifyicon
            notifyIcon.ContextMenu = contextMenu;

            // set the styles of this form to make it go a little faster with drawing
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // start and setup all the used classes
            WINDOW_HANDLER = new WindowHandler().ConstructVariables(this);
            RUNEDOKU_SOLUTION = new RuneDokuSolution().ConstructVariables(this);
            HOOK_HANDLER = new HookHandler().ConstructVariables(this);

            // set up all the timers
            ProgramLoop = new Timer();
            ProgramLoop.Interval = 1;
            ProgramLoop.Tick += new EventHandler(MainProgramLoop);
            // start the main program loop
            ProgramLoop.Start();
        }

        /// <summary>
        /// The main loop of the program
        /// this will dictate the current step of what's happening in the program
        /// </summary>
        private void MainProgramLoop(object sender, EventArgs e)
        {
            // Check to see if the user has selected the RSWindow to be used
            // if not make the user selects it before the program starts
            if (RSWindowHandle == IntPtr.Zero)
            {
                if (HOOK_HANDLER.grabWindow)
                {
                    string windowProcName = GetActiveProcessFileName();
                    if (windowProcName != "OSBuddy.exe" && windowProcName != "Jagex Launcher.exe")
                    {
                        notifyIcon.BalloonTipText = $"The window, {windowProcName}, you're trying to grab is not a runescape window! It needs to be the regular Runescape client or the OSBuddy client.";
                        notifyIcon.ShowBalloonTip(10);
                    } else
                    {
                        RSWindowHandle = GetForegroundWindow();
                        notifyIcon.BalloonTipText = $"{windowProcName} grabbed!";
                        notifyIcon.ShowBalloonTip(10);
                    }                    
                }
            }
            else
            {
                // the first step to the program is to check if the RuneDoku Window is open on the 
                // Runescape screen
                if (WINDOW_HANDLER.SolveButtonForm.IsDisposed)
                {
                    // if the solve button form isn't open then that means the program should be
                    // looking for the runedoku window to be open
                    WINDOW_HANDLER.RunedokuWindowChecker();
                }
                // next step is to check if the solve button form is visisble
                // if it is that means we need to start checking for the user clicking on any
                // of the availible buttons
                else if (WINDOW_HANDLER.SolveButtonForm.Visible)
                {
                    if (WINDOW_HANDLER.close)
                    {
                        if (!WINDOW_HANDLER.CheckScreenPixel(new Point(489, 320), Color.FromArgb(98, 44, 12), WINDOW_HANDLER.GetRSWindowRect()))
                        {
                            WINDOW_HANDLER.SolveButtonForm.Dispose();
                            if (WINDOW_HANDLER.runeDokuWindow.Visible)
                                WINDOW_HANDLER.runeDokuWindow.Dispose();

                            WINDOW_HANDLER.close = false;
                        }                        
                    }
                    else
                    {
                        // then run the button handler function to check if the player presses any of the
                        // buttons
                        WINDOW_HANDLER.RunedokuWindowButtonHandler();

                        // update the solve buttons position based on the runescape window
                        WINDOW_HANDLER.UpdateForm(WINDOW_HANDLER.SolveButtonForm, WINDOW_HANDLER.buttonList[0]);
                        // check to see if the RuneDoku solution is visible
                        // if so update the position of it
                        if (WINDOW_HANDLER.runeDokuWindow.Visible)
                            WINDOW_HANDLER.UpdateForm(WINDOW_HANDLER.runeDokuWindow, new Rectangle(113, 37, 343, 334));
                    }
                }
            }
        }

        /// <summary>
        /// Retrive the title of the currently active window
        /// </summary>
        /// <returns>The title of the active window</returns>
        public string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        /// <summary>
        /// Retrive the name of the active windows process
        /// </summary>
        /// <returns>The process name of the active window</returns>
        public string GetActiveProcessFileName()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            Process activeProcess = Process.GetProcessById((int)pid);
            string[] processPath = activeProcess.MainModule.FileName.Split('\\');
            return processPath[processPath.Length-1];
        }

        /// <summary>
        /// This will close the program.
        /// Called when the exit button on the notifyicon context menu is clicked.
        /// </summary>
        private void CloseProgram(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            ProgramLoop.Stop();
            this.Close();
        }

        /// <summary>
        /// Find out if the user closes the program from the form window
        /// </summary>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon.Dispose();
            ProgramLoop.Stop();
        }
    }
}
