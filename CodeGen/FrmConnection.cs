using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeGen
{
    public partial class FrmConnection : Form
    {
        public FrmConnection()
        {
            InitializeComponent();
        }

        string configPath = "";// 配置路径
        IniFileHelper iniHelpr;// 配置帮助类
        Dictionary<string, DbParam> configList = new Dictionary<string, DbParam>();//配置字典
        public DbParam selectDbParm = new DbParam();//选中的数据库配置

        private void FrmConnection_Load(object sender, EventArgs e)
        {
            InitDBType();
            LoadConfig();
        }

        /// <summary>
        /// 初始化数据库下拉列表
        /// </summary>
        private void InitDBType()
        {
            cboDbType.Items.Add("SqlServer");
            cboDbType.Items.Add("MySql");
            cboDbType.Items.Add("Oracle");
            cboDbType.Items.Add("PostgreSQL");
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private void LoadConfig()
        {
            // 读取同目录下的config.ini文件
            configPath = System.Threading.Thread.GetDomain().BaseDirectory + "config.ini";
            if (!File.Exists(configPath))
            {
                MessageBox.Show("程序目录下没有找到配置文件 config.ini");
                return;
            }

            iniHelpr = new IniFileHelper(configPath);
            configList.Clear();

            StringBuilder sb = new StringBuilder(60);


            readDbConfigByType("SqlServer");
            readDbConfigByType("MySql");
            readDbConfigByType("Oracle");
            readDbConfigByType("PostgreSQL");
            iniHelpr.GetIniString("Common", "defaultDbIndex", "0", sb, sb.Capacity);
            cboDbType.SelectedIndex = Convert.ToInt32(sb.ToString());
            cboDbType_SelectedIndexChanged(null, null);
        }

        /// <summary>
        /// 根据数据库类型读取ini配置值
        /// </summary>
        /// <param name="dbType"></param>
        private void readDbConfigByType(string dbType)
        {
            DbParam dbParam = new DbParam();
            StringBuilder sb = new StringBuilder(60);
            if (iniHelpr.GetIniString(dbType, "host", "", sb, sb.Capacity))
            {
                dbParam.Host = sb.ToString();
            }
            if (iniHelpr.GetIniString(dbType, "port", "", sb, sb.Capacity))
            {
                dbParam.Port = sb.ToString();
            }
            if (iniHelpr.GetIniString(dbType, "uid", "", sb, sb.Capacity))
            {
                dbParam.UserId = sb.ToString();
            }
            if (iniHelpr.GetIniString(dbType, "password", "", sb, sb.Capacity))
            {
                dbParam.Password = sb.ToString();
            }
            if (iniHelpr.GetIniString(dbType, "dbname", "", sb, sb.Capacity))
            {
                dbParam.DbName = sb.ToString();
            }

            configList.Add(dbType, dbParam);
        }

        /// <summary>
        /// 根据类型过滤端口输入框 有些数据库连接不需要端口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboDbType_SelectedIndexChanged(object sender, EventArgs e)
        {

            string type = cboDbType.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(type))
            {
                txtBoxDbName.Enabled = true;
                txtBoxHost.Enabled = true;
                txtBoxPassword.Enabled = true;
                txtBoxPort.Enabled = true;
                txtBoxUserId.Enabled = true;
                chkShowPassword.Enabled = true;
            }
            else
            {
                txtBoxDbName.Enabled = false;
                txtBoxHost.Enabled = false;
                txtBoxPassword.Enabled = false;
                txtBoxPort.Enabled = false;
                txtBoxUserId.Enabled = false;
                chkShowPassword.Enabled = false;

                return;
            }


            // 读取配置 自动赋值
            if (configList.TryGetValue(type, out DbParam db))
            {
                txtBoxUserId.Text = db.UserId;
                txtBoxPassword.Text = db.Password;
                txtBoxHost.Text = db.Host;

                if (type.Equals("SqlServer") || type.Equals("Oracle"))
                {
                    txtBoxPort.Text = "";
                    txtBoxPort.Enabled = false;
                }
                else
                {
                    txtBoxPort.Text = db.Port;
                    txtBoxPort.Enabled = true;
                }
                txtBoxDbName.Text = db.DbName;

                selectDbParm = db;
            }
        }

        /// <summary>
        /// 确定按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConn_Click(object sender, EventArgs e)
        {
            if (cboDbType.SelectedIndex < 0)
            {
                MessageBox.Show("请选择一个数据库");
                return;
            }

            selectDbParm.Host = txtBoxHost.Text;
            selectDbParm.Port = txtBoxPort.Text;
            selectDbParm.UserId = txtBoxUserId.Text;
            selectDbParm.Password = txtBoxPassword.Text;
            selectDbParm.DbName = txtBoxDbName.Text;
            selectDbParm.DbType = cboDbType.SelectedItem.ToString();

            //保存配置
            WriteDbConfigByType(selectDbParm);
            iniHelpr.WriteIniString("Common", "defaultDbIndex", cboDbType.SelectedIndex.ToString());

            DialogResult = DialogResult.Yes;
        }

        /// <summary>
        /// 根据数据库类型读取ini配置值
        /// </summary>
        /// <param name="dbType"></param>
        private void WriteDbConfigByType(DbParam dbParam)
        {
            iniHelpr.WriteIniString(dbParam.DbType, "host", dbParam.Host);

            if (dbParam.DbType.Equals("SqlServer"))
            {
                iniHelpr.WriteIniString(dbParam.DbType, "port", "");
            }
            else
            {
                iniHelpr.WriteIniString(dbParam.DbType, "port", dbParam.Port);
            }

            iniHelpr.WriteIniString(dbParam.DbType, "uid", dbParam.UserId);
            iniHelpr.WriteIniString(dbParam.DbType, "password", dbParam.Password);
            iniHelpr.WriteIniString(dbParam.DbType, "dbname", dbParam.DbName);
        }

        /// <summary>
        /// 取消按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowPassword.Checked)
            {
                txtBoxPassword.PasswordChar = '\0';
            }
            else
            {
                txtBoxPassword.PasswordChar = '*';
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
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
