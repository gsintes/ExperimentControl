using System;
using System.Configuration;
using System.Windows.Forms;

namespace ExperimentControl.ExperimentControl
{
    /// <summary>
    /// Main form that is used to select the experiement and then closed
    /// </summary>
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.comboBox1.DataSource = Enum.GetValues(typeof(Experiments.Experiments));

            this.KeyUp += Form2_KeyUp;
        }
        /// <summary>
        /// Launch the experiment when Enter is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Key pressed</param>
        private void Form2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Launch();
            }
        }
        /// <summary>
        /// Launch the experiment when the Launch button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchButton_Click(object sender, EventArgs e)
        {
            Launch();
        }
        /// <summary>
        /// Launch the experiement chosen in the comboBox in a 2nd form
        /// </summary>
        private void Launch()
        {
            Enum.TryParse<Experiments.Experiments>(comboBox1.SelectedValue.ToString(), out Experiments.Experiments exp);
            using Form1 form1 = new Form1(exp);
            Hide();
            form1.ShowDialog();
            Close();
        }
        /// <summary>
        /// Open the ExpInfo file in the default navigator (it is a html file)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InfoButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(ConfigurationManager.AppSettings["ExpInfoFile"]);
        }
    }
}
