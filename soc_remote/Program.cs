using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GSoft.Utility;
using System.IO;
namespace soc_remote
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            #if _DEBUG
            //Directory::SetCurrentDirectory(String::Format("{0}/../Release", AppDomain.CurrentDomain->BaseDirectory));
             Directory.SetCurrentDirectory(String.Format("{0}/../Release",AppDomain.CurrentDomain.BaseDirectory);
#else
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
#endif

//#ifndef _DEBUG
        //try {
//#endif
		// check multiple instances
		if (!ProcessU.multiple(args)) {
			System.Environment.Exit(0);
		}

		// check open console
		if (Array.IndexOf(args, "-debug") >= 0) {
			// create but not log
			ConsoleU.createConsole();
			ConsoleU.hide();
		} 

		// check log file
		if (Array.IndexOf(args, "-log") >= 0) {			
			ConsoleU.createConsole(true);
			ConsoleU.hide();
		}


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new gf_main(args));
        }
    }
}
