using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ExperimentControl
{
    public partial class Form1 : Form
    {
        Control control;
        public Form1()
        {
           

            control = new Control();
            InitializeComponent();
            
            refreshTimer.Interval = 100;
            refreshTimer.Start();
            
        }


        private void StopButton_Click(object sender, EventArgs e)
        {
            DialogResult rsl = MessageBox.Show("You are going to stop the experiment?", "Stop experiment", MessageBoxButtons.OKCancel);
            switch (rsl)
            {
                case DialogResult.OK:

                    control.Stop();
                    break;
                case DialogResult.Cancel:
                    MessageBox.Show("Information", "Experiment continues");
                    break;
                default:
                    MessageBox.Show("Information", "Experiment continues");
                    break;
            }


        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            control.Start();
        }

        private void Refreshtimer_Tick(object sender, EventArgs e)
        {
            laserTxtBox.Text = "Laser Shutter:" + (control.GetShutterState() ? "OPEN" : "CLOSE");
            laserTxtBox.BackColor = control.GetShutterState() ? Color.Green : Color.Red;
            lampTxtBox.Text = "Main lamp:" + (control.GetLampState() ? "DAY" : "NIGHT");
            lampTxtBox.BackColor = control.GetLampState() ? Color.Yellow : Color.Gray;
            redLampTxtBox.Text = "Red lamp" + (control.GetRedLampState() ? "ON" : "OFF");
            redLampTxtBox.BackColor = control.GetRedLampState() ? Color.Green : Color.Red;
        }
    }
}
