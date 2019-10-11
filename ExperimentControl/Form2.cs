using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExperimentControl
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.comboBox1.DataSource = Enum.GetValues(typeof(Experiments.Experiments));
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void LaunchButton_Click(object sender, EventArgs e)
        {
            Experiments.Experiments exp;
            Enum.TryParse<Experiments.Experiments>(comboBox1.SelectedValue.ToString(), out exp);
            Form1 form1 = new Form1(exp);
            Hide();
            form1.ShowDialog();
            Close();
        }

       
    }
}
