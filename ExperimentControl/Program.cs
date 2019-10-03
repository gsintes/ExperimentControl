using System;
using System.IO;
using System.Windows.Forms;

namespace ExperimentControl
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Directory.SetCurrentDirectory("C:/Users/gs656local/Documents/ExperimentResults");
            DateTime date = DateTime.Now;
            string dirName = string.Format("{0}-{1}-{2}_Experiment",
                date.Year,
                date.Month,
                date.Day);
            
            Directory.CreateDirectory(dirName);
            Directory.SetCurrentDirectory(dirName);
            Directory.CreateDirectory("TankPictures");
            using (_ = File.Create("log.txt"))
            {
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
