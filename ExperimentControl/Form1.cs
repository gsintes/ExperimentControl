using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ExperimentControl
{
    /// <summary>
    /// Form  displayed while the experiment is running
    /// </summary>
    public partial class Form1 : Form
    {
        private readonly Experiments.IControl control;
        public Form1()
        {
            try
            {
                InitializeComponent();
                refreshTimer.Interval = 1000;
                refreshTimer.Start();
                control = new Experiments.Experiment2();
            }
            catch (FormatNotRespectedException)
            {
                MessageBox.Show("The setting file doesn't respect the format, modify it and retry.", "Error", MessageBoxButtons.OK);
            }
            catch (NoCameraDetectedException)
            {
                MessageBox.Show("No camera detected. Try force the IP in FlyCap2.", "Error", MessageBoxButtons.OK);
            }
        }
        public Form1(Experiments.Experiments exp)
        {
            
            try
            {
                InitializeComponent();
                refreshTimer.Interval = 1000;
                refreshTimer.Start();
                switch (exp)
                {
                    case Experiments.Experiments.Exp1:
                        control = new Experiments.Experiment1();
                        break;
                    case Experiments.Experiments.Exp2:
                        control = new Experiments.Experiment2();
                        break;
                }
                
            }
            catch (FormatNotRespectedException)
            {
                MessageBox.Show("The setting file doesn't respect the format, modify it and retry.", "Error", MessageBoxButtons.OK);
            }
            catch (NoCameraDetectedException)
            {
                MessageBox.Show("No camera detected. Try force the IP in FlyCap2.", "Error", MessageBoxButtons.OK);
            }
            this.Name = exp.ToString();
            this.Text = exp.ToString();
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
