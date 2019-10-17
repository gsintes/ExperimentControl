﻿using System;
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
        }

        private void LaunchButton_Click(object sender, EventArgs e)
        {
            Enum.TryParse<Experiments.Experiments>(comboBox1.SelectedValue.ToString(), out Experiments.Experiments exp);
            using Form1 form1 = new Form1(exp);
            Hide();
            form1.ShowDialog();
            Close();
        }

        private void InfoButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(ConfigurationManager.AppSettings["ExpInfoFile"]);
        }
    }
}
