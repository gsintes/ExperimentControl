using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ExperimentControl
{
    public partial class Form1 : Form
    {
        private IControl control;
        public Form1()
        {
            
            try
            {
                
                control = new Control();
                InitializeComponent();
                refreshTimer.Interval = 1000;
                refreshTimer.Start();
                
            }
            catch( FormatNotRespectedException )
            {
                MessageBox.Show("The setting file doesn't respect the format, modify it and retry.","Error",MessageBoxButtons.OK);
            }
            catch (NoCameraDetectedException)
            {
                MessageBox.Show("No camera detected. Try force the IP in FlyCap2.", "Error", MessageBoxButtons.OK);

            }
        }


        private void StopButton_Click(object sender, EventArgs e)
        {
            if (control.Running)
            {
                DialogResult rsl = MessageBox.Show("You are going to stop the experiment?", "Stop experiment", MessageBoxButtons.OKCancel);
                switch (rsl)
                {
                    case DialogResult.OK:
                        Thread thread = new Thread(control.Stop);
                        thread.Start();
                        break;
                    case DialogResult.Cancel:
                        MessageBox.Show("Information", "Experiment continues");
                        break;
                    default:
                        MessageBox.Show("Information", "Experiment continues");
                        break;
                }
            }


        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (!control.Running)
            {
                Thread thread = new Thread(control.Start);
                thread.Start();

            }
            
        }

        private void Refreshtimer_Tick(object sender, EventArgs e)
        {
            laserTxtBox.Text = "Laser Shutter: " + (control.GetShutterState() ? "OPEN" : "CLOSE");
            laserTxtBox.BackColor = control.GetShutterState() ? Color.Green : Color.Red;
            lampTxtBox.Text = "Main lamp: " + (control.GetLampState() ? "DAY" : "NIGHT");
            lampTxtBox.BackColor = control.GetLampState() ? Color.Yellow : Color.Gray;
            redLampTxtBox.Text = "Red lamp: " + (control.GetRedLampState() ? "ON" : "OFF");
            redLampTxtBox.BackColor = control.GetRedLampState() ? Color.Green : Color.Red;
        }
    }
}
