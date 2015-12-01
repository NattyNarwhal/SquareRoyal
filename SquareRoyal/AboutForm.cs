using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

namespace SquareRoyal
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            var fv = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            versionLabel.Text = String.Format("{0}, by {1}", fv.ProductVersion, fv.CompanyName);
            titleLabel.Text = fv.ProductName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            ((MainForm)Owner).AllowCheating();
            MessageBox.Show(this, "Cheeky! If you want to play honestly, restart the game and pretend you never saw this.", "You're a cheater!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
