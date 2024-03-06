using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SAdK____Unlock_it_
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SAdKUnlockIt());
        }
    }
}
