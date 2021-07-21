using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] Args)
        {



            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new frmClient());
            frmClient f = new frmClient();
            if (Args.Length > 0)
            {
                Global.User = Args[0].ToString();

            }

            Application.Run(new frmClient());


        }
    }
}
