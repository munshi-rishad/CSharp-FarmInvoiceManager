using System;
using System.Windows.Forms;

namespace BanglaProject
{
    internal static class Program
    {
        // The application starts with the login window
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormLogin());
        }
    }
}
