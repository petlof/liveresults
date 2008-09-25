using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WOCEmmaClient
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
            //FrmNewCompetition cmp = new FrmNewCompetition();
            //if (cmp.ShowDialog() == DialogResult.Cancel)
            //{
            //    return;
            //}
            Application.Run(new FrmNewCompetition());
            return;
            
#if OLA
            Application.Run(new OlaForm());
#elif SITimec
            Application.Run(new SITimecForm());
#elif OE
            Application.Run(new OEForm());
#else
#if OS
            Application.Run(new OSSpeakerForm());
#else
            Application.Run(new Form1());
#endif
#endif
        }
    }
}