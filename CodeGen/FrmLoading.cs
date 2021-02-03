using System;
using System.Windows.Forms;

namespace CodeGen
{
    public partial class FrmLoading : Form
    {
        public FrmLoading()
        {
            InitializeComponent();
        }

        public void SetMsg(string msg, int progress)
        {
            if (label1.InvokeRequired)
            {
                label1.BeginInvoke(new Action<string>(m => label1.Text = m), new object[] { msg });
                progressBar1.BeginInvoke(new Action<int>(m => progressBar1.Value = m), new object[] { progress });
                label3.BeginInvoke(new Action<int>(m => label3.Text = m.ToString()), new object[] { progress });
            }
            else
            {
                label1.Text = msg;
                progressBar1.Value = progress;
                label3.Text = progress.ToString();
            }
        }
    }
}
