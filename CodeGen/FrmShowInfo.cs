using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CodeGen
{
    public partial class FrmShowInfo : Form
    {
        public FrmShowInfo()
        {
            InitializeComponent();
            button1.Focus();
        }

        public void SetMsg(string msg)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.BeginInvoke(new Action<string>(m => textBox1.Text = msg), new object[] { msg });
            }
            else
            {
                textBox1.Text = msg;
            }

        }

        private void ShowInfo_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            button1.ForeColor = Color.White;
            button1.BackColor = Color.Red;
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.ForeColor = Color.Black;
            button1.BackColor = Color.White;
        }

        private const int VM_NCLBUTTONDOWN = 0XA1;//定义鼠标左键按下
        private const int HTCAPTION = 2;
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            //为当前应用程序释放鼠标捕获
            ReleaseCapture();
            //发送消息 让系统误以为在标题栏上按下鼠标
            SendMessage((IntPtr)this.Handle, VM_NCLBUTTONDOWN, HTCAPTION, 0);
        }
    }
}
