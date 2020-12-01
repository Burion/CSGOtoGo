using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DemoInfo;


namespace CSGOtoGo
{
    public partial class MainForm : Form
    {   
        
        OpenFileDialog openFileDialog;
        public MainForm()
        {
            this.SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.DoubleBuffer,true);
            this.WindowState = FormWindowState.Maximized;
            Button browse = new Button() { Text = "Browse", Height = 100, Width = 200};
            browse.Click += fileButtonClick;
            openFileDialog = new OpenFileDialog();
            Controls.Add(browse);
            

        }
        private void fileButtonClick(object sender, EventArgs e)
        {
            int size = -1;
            DialogResult result = openFileDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog.FileName;
                try
                {
                    Form1 form = new Form1(file);
                    form.Show();
                }
                catch (IOException)
                {
                }
            }
        }
    }
}
