using System;
using System.IO;
using System.Windows.Forms;
using System.Configuration;


namespace ExperimentControl.ExperimentControl
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
           
            Directory.SetCurrentDirectory(ConfigurationManager.AppSettings["Folder"]);
            DateTime date = DateTime.Now;
            string dirName = string.Format("{0}-{1}-{2}_Experiment",
                date.Year,
                date.Month,
                date.Day);
            string dirZero = dirName;
            int i = 1;
            while (Directory.Exists(dirName))
            {
                dirName = dirZero + string.Format("_{0}", i);
                i++;
            }
            Directory.CreateDirectory(dirName);
            Directory.SetCurrentDirectory(dirName);
            Directory.CreateDirectory("TankPictures");
            using (_ = File.Create("log.txt"))
            {
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ExperimentControl.Form2());
        }
    }
}
