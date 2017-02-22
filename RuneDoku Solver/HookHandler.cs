using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RuneDoku_Solver
{
    public class HookHandler
    {

        // Parent script
        public Form1 PARENT_SCRIPT;

        // Hook Variables
        public IKeyboardMouseEvents m_GlobalHook;

        // ButtonVariables
        public bool lmbClicked = false;
        public bool escPressed = false;

        /// <summary>
        /// Construct all the variables needed for this class
        /// </summary>
        /// <param name="parent">The Parent Script Used To Access Other Classes</param>
        /// <returns>The created HookHandler Class</returns>
        public HookHandler ConstructVariables(Form1 parent)
        {
            // set up the parent script
            PARENT_SCRIPT = parent;

            // set up the global hooks
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.MouseDownExt += MouseClicked;
            m_GlobalHook.MouseUpExt += MouseUnClicked;

            m_GlobalHook.KeyUp += KeyUpCheck;
            m_GlobalHook.KeyDown += KeyDownCheck;

            // now return this script to be stored
            return this;
        }

        /// <summary>
        /// Checks to see if the user has clicked the mouse down
        /// </summary>
        private void MouseClicked(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lmbClicked = true;
            }
        }

        /// <summary>
        /// Checks to see if the user has rleased the mouse button
        /// </summary>
        private void MouseUnClicked(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lmbClicked = false;
            }
        }

        /// <summary>
        /// Checks to see if the player has pressed a key on the keyboard down
        /// </summary>
        private void KeyUpCheck(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                escPressed = false;
            }
        }

        /// <summary>
        /// Checks to see if the player has rleased a key on the keyboard
        /// </summary>
        private void KeyDownCheck(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                escPressed = true;
            }
        }
    }
}
