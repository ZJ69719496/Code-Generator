using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CodeGen
{
    public partial class FrmCommon : Form
    {
        public FrmCommon()
        {
            InitializeComponent();
            LoadConfig();
        }

        string configPath = "";// 配置路径
        IniFileHelper iniHelpr;// 配置帮助类

        /// <summary>
        /// 加载配置
        /// </summary>
        private void LoadConfig()
        {
            // 读取目录下的config.ini文件
            configPath = System.Threading.Thread.GetDomain().BaseDirectory + "config.ini";
            if (!File.Exists(configPath))
            {
                //MessageBox.Show("程序目录下没有找到配置文件 config.ini");
                File.Create(configPath);
            }
            else
            {
                iniHelpr = new IniFileHelper(configPath);
                ReadCommonConfig();
            }
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        private void ReadCommonConfig()
        {
            StringBuilder sb = new StringBuilder(60);
            if (iniHelpr.GetIniString("Common", "company", "", sb, sb.Capacity))
            {
                textBox4.Text = sb.ToString();
            }
            if (iniHelpr.GetIniString("Common", "partment", "", sb, sb.Capacity))
            {
                textBox5.Text = sb.ToString();
            }
            if (iniHelpr.GetIniString("Common", "apiVersion", "", sb, sb.Capacity))
            {
                textBox6.Text = sb.ToString();
            }
            if (iniHelpr.GetIniString("Common", "model", "", sb, sb.Capacity))
            {
                textBox7.Text = sb.ToString();
            }
        }

        /// <summary>
        /// 关闭按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        private void WriteCommonConfig()
        {
            iniHelpr.WriteIniString("Common", "company", textBox4.Text);
            iniHelpr.WriteIniString("Common", "partment", textBox5.Text);
            iniHelpr.WriteIniString("Common", "apiVersion", textBox6.Text);
            iniHelpr.WriteIniString("Common", "model", textBox7.Text);
        }

        public void SetSelectCount(int count)
        {
            label3.Text = count.ToString() + " 张表";
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            WriteCommonConfig();
            if (CallBack != null)
            {
                CallBack.Invoke(textBox4.Text, textBox5.Text, textBox6.Text, textBox7.Text);
            }
            Close();
        }

        public delegate void setCommonHanlder(string s1, string s2, string s3, string s4);
        public event setCommonHanlder CallBack;

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
