using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SquareRoyal
{
    static class Program
    {
        // 0 = unaware, 1 = single monitor, 2 = multi-monitor
        [DllImport("shcore.dll")]
        static extern int SetProcessDpiAwareness(int value);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Environment.OSVersion.Version > new Version(6, 2) &&
                Environment.OSVersion.Platform == PlatformID.Win32NT)
                SetProcessDpiAwareness(1);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
