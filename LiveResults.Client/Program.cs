using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LiveResults.Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //using (var sirapInterface = new SirapInterface())
            {
              //  sirapInterface.Start();
                Application.Run(new FrmNewCompetition());
                //sirapInterface.Stop();
            }

        }
    }
}