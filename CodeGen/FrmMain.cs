using SqlSugar;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace CodeGen
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 填写的数据库连接参数
        /// </summary>
        DbParam dbparam;
        /// <summary>
        /// 数据库客户端
        /// </summary>
        SqlSugarClient dbClient = null;
        /// <summary>
        /// 需要生成的库中的所有表
        /// </summary>
        DataTable tables = null;
        readonly Dictionary<string, TableEntity> tableList = new Dictionary<string, TableEntity>();
        readonly Dictionary<string, DataTable> tableStructsList = new Dictionary<string, DataTable>();
        List<FileInfo> templateFiles = null;

        /// <summary>
        /// 生成器信息
        /// </summary>
        string GenInfo = "CodeGen v1.0 for DotNet Core";
        /// <summary>
        /// 公司名称
        /// </summary>
        string Company = "苏州宏软";
        /// <summary>
        /// 部门名称
        /// </summary>
        string Department = "项目开发部门";
        /// <summary>
        /// 生成的模块名称
        /// </summary>
        string ModelName = "UserCenter";
        /// <summary>
        /// 生成的api版本
        /// </summary>
        string ApiVersion = "1";
        /// <summary>
        /// 需要生成表的数量
        /// </summary>
        int selectCount = 0;

        FrmLoading load = null;

        public void SetCommon(string company, string department, string apiVersion, string modelName)
        {
            Company = company;
            Department = department;
            ApiVersion = apiVersion;
            ModelName = modelName;
            button2.Enabled = true;
        }


        private async void GetTableInfo()
        {
            try
            {
                // 获取表名、表注释
                tables = await GetTableNameAndComment(dbparam.DbType);
                if (tables.Rows.Count == 0)
                {
                    btnGenCode.Enabled = false;
                    button4.Enabled = false;
                    MessageBox.Show("没有找到表");
                    return;
                }


                Success("获取表名和表注释成功");
                Success("表数量 ：" + tables.Rows.Count);

                dgvTableList.DataSource = tables;
                checkBox1.Enabled = true;
                button5.Enabled = true;

                // 如果没有增加过
                //if (!flag)
                //{
                //    DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
                //    checkBoxColumn.Name = "select";
                //    checkBoxColumn.HeaderText = "选择";
                //    dgvTableList.Columns.Add(checkBoxColumn);
                //    flag = true;
                //}


                lblTableCount.Text = tables.Rows.Count.ToString();

                tableList.Clear();
                tableStructsList.Clear();

                if (load == null)
                {
                    load = new FrmLoading();
                }


                load.Show();

                Split();
                Success("开始解析表信息");

                int count = tables.Rows.Count;
                double current = 0.0;
                int process = 0;
                await Task.Run(async () =>
                {
                    // 遍历表列表
                    foreach (DataRow dr in tables.Rows)
                    {
                        current += 1;
                        string tableName = dr["name"].ToString();
                        string tableComment = dr["comment"].ToString();

                        TableEntity table = new TableEntity
                        {
                            TableComment = tableComment,
                            TableName = tableName
                        };

                        process = Convert.ToInt32(Math.Floor(current / count * 100));
                        load.SetMsg(tableName, process);
                        await GetCols(table, dbparam);
                        tableList.Add(table.TableName, table);
                        Success(tableName);
                    }
                });

                load.Hide();

                Split();
                Info("表信息解析完成");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }



        private string GetDbListSql(string type)
        {
            string sql = "";
            switch (type)
            {
                case "MySql":
                    sql = "SELECT SCHEMA_NAME AS `Database` FROM INFORMATION_SCHEMA.SCHEMATA;";
                    break;
                case "SqlServer":
                    sql = "Select Database FROM Master..SysDatabases orDER BY Name";
                    break;
                case "Sqlite":
                    sql = "";
                    break;
                case "Oracle":
                    sql = "select * from user_tables;";
                    //sql = "select * from tabs;";
                    //sql = "select * from all_tables where owner='VTATEST';";
                    break;
                case "PostgreSQL":
                    sql = "select tablename from pg_tables where schemaname='public'";
                    break;
                case "Dm":
                    sql = "";
                    break;
                case "Kdbndp":
                    sql = "";
                    break;
                default:
                    break;
            }
            return sql;
        }

        /// <summary>
        /// 获取查询表名称和表注释
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task<DataTable> GetTableNameAndComment(string type)
        {
            string sql;
            object o = new { };
            switch (type)
            {
                case "MySql":
                    sql = "SELECT TABLE_NAME as name,TABLE_COMMENT as comment FROM information_schema.TABLES WHERE table_schema=@dbName;";
                    o = new { dbName = dbparam.DbName };
                    break;
                case "SqlServer":
                    sql = @"SELECT tbs.name name, ds.value comment       
                            FROM sys.extended_properties ds  
                            LEFT JOIN sysobjects tbs 
                            ON ds.major_id = tbs.id  
                            WHERE ds.minor_id = 0;";
                    o = new { };
                    break;
                case "Sqlite":
                    sql = "";
                    break;
                case "Oracle":
                    sql = "select * from user_tables;";
                    break;
                case "PostgreSQL":
                    sql = "select tablename from pg_tables where schemaname='public'";
                    break;
                case "Dm":
                    sql = "";
                    break;
                case "Kdbndp":
                    sql = "";
                    break;
                default:
                    sql = "";
                    break;
            }
            return await dbClient.Ado.GetDataTableAsync(sql, o);
        }

        /// <summary>
        /// 获取查询表结构
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tableName"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        private async Task<DataTable> GetTableStruct(string tableName, DbParam db)
        {
            DataTable dt = null;
            string sql;
            switch (db.DbType)
            {
                case "MySql":
                    sql = @"SELECT 
                            COLUMN_NAME AS Name, 
                            DATA_TYPE AS Type, 
                            IFNULL(CHARACTER_MAXIMUM_LENGTH , 0) AS TypeLength,
                            COLUMN_COMMENT AS Comment,
                            IS_NULLABLE AS nullable,
                            COLUMN_KEY AS pk
                            FROM INFORMATION_SCHEMA.Columns WHERE table_name = @tableName AND table_schema = @dbName;";
                    dt = await dbClient.Ado.GetDataTableAsync(sql, new { tableName, dbName = db.DbName });
                    break;
                case "SqlServer":
                    sql = @"SELECT col.name AS Name ,  
                                    ISNULL(ep.[value], '') AS Comment,  
                                    t.name AS Type, 
                                    col.length AS TypeLength,
                                    CASE WHEN EXISTS ( SELECT   1  
                                                       FROM     dbo.sysindexes si  
                                                                INNER JOIN dbo.sysindexkeys sik ON si.id = sik.id  
                                                                                          AND si.indid = sik.indid  
                                                                INNER JOIN dbo.syscolumns sc ON sc.id = sik.id  
                                                                                          AND sc.colid = sik.colid  
                                                                INNER JOIN dbo.sysobjects so ON so.name = si.name  
                                                                                          AND so.xtype = 'PK'  
                                                       WHERE    sc.id = col.id  
                                                                AND sc.colid = col.colid ) THEN 'PRI'  
                                         ELSE ''  
                                    END AS pk ,  
                                    CASE WHEN col.isnullable = 1 THEN 'true'  
                                         ELSE 'false'  
                                    END AS nullable
                            FROM    dbo.syscolumns col  
                                    LEFT  JOIN dbo.systypes t ON col.xtype = t.xusertype  
                                    inner JOIN dbo.sysobjects obj ON col.id = obj.id  
                                                                     AND obj.xtype = 'U'  
                                                                     AND obj.status >= 0  
                                    LEFT  JOIN dbo.syscomments comm ON col.cdefault = comm.id  
                                    LEFT  JOIN sys.extended_properties ep ON col.id = ep.major_id  
                                                                                  AND col.colid = ep.minor_id  
                                                                                  AND ep.name = 'MS_Description'  
                                    LEFT  JOIN sys.extended_properties epTwo ON obj.id = epTwo.major_id  
                                                                                     AND epTwo.minor_id = 0  
                                                                                     AND epTwo.name = 'MS_Description'  
                            WHERE   obj.name = @tableName  
                            ORDER BY col.colorder;";
                    dt = await dbClient.Ado.GetDataTableAsync(sql, new { tableName });
                    break;
                case "Sqlite":
                    sql = "";
                    break;
                case "Oracle":
                    sql = "";
                    break;
                case "PostgreSQL":
                    sql = "";
                    break;
                case "DbType.Dm":
                    sql = "";
                    break;
                case "Kdbndp":
                    sql = "";
                    break;
                default:
                    break;
            }
            return dt;
        }

        /// <summary>
        /// 根据表名称获取所有字段列表信息
        /// </summary>
        /// <param name="table"></param>
        /// <param name="tableName"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        private async Task GetCols(TableEntity table, DbParam db)
        {
            DataTable dt = await GetTableStruct(table.TableName, db);

            if (dt.Rows.Count < 0)
            {
                MessageBox.Show("没有找到表结构哦");
                return;
            }

            tableStructsList.Add(table.TableName, dt);

            Dictionary<string, ColumnEntity> list = new Dictionary<string, ColumnEntity>();

            foreach (DataRow dr in dt.Rows)
            {
                var a = dr["nullable"].ToString().ToLower().Equals("YES") || dr["nullable"].ToString().ToLower().Equals("true");
                var col = new ColumnEntity
                {
                    ColumnName = dr["Name"].ToString(),
                    ColumnType = dr["Type"].ToString(),
                    columnComment = dr["Comment"].ToString(),
                    IsNull = dr["nullable"].ToString().ToLower().Equals("yes") || dr["nullable"].ToString().ToLower().Equals("true"),
                    IsPk = dr["pk"].ToString().Equals("PRI")
                };

                if (int.TryParse(dr["TypeLength"].ToString(), out int length))
                {
                    col.columnTypeLength = length;
                }

                if (col.IsPk)
                {
                    table.pkColumnName = col.ColumnName;
                    table.pkColumnComment = col.columnComment;
                    table.pkColumnType = col.ColumnType;
                    table.pkColumnTypeLength = col.columnTypeLength;
                }
                list.Add(dr["Name"].ToString(), col);
            }
            table.Cols = list;
        }


        #region 获取数据库客户端

        /// <summary>
        /// 获取数据库连接实例
        /// </summary>
        /// <param name="DB"></param>
        /// <returns></returns>
        private SqlSugarClient GetDBInstance(DbParam DB)
        {
            SqlSugarClient db = null;
            string ConnectionString;
            switch (DB.DbType)
            {
                case "MySql":
                    ConnectionString = string.Format("server={0};Uid={1};Pwd={2};Database={3}", DB.Host, DB.UserId, DB.Password, DB.DbName);
                    db = GetMySqlInstance(ConnectionString);
                    break;
                case "SqlServer":
                    ConnectionString = string.Format("server={0};uid={1};pwd={2};database={3}", DB.Host, DB.UserId, DB.Password, DB.DbName);
                    db = GetSqlServerInstance(ConnectionString);
                    break;
                case "Sqlite":
                    MessageBox.Show("我不想做这个数据库的生成！");
                    break;
                case "Oracle":
                    ConnectionString = string.Format("Data Source={0}/{1};User ID={2};Password={3};", DB.Host, DB.DbName, DB.UserId, DB.Password);
                    db = GetOracleInstance(ConnectionString);
                    break;
                case "PostgreSQL":
                    ConnectionString = string.Format("HOST={0};PORT={1};USER ID={2};PASSWORD={3};DATABASE={4}", DB.Host, DB.Port, DB.UserId, DB.Password, DB.DbName);
                    db = GetPostgreSQLInstance(ConnectionString);
                    break;
                case "Dm":
                    ConnectionString = string.Format("Server={0};Port={1};UID={2};PWD={3};database={4}", DB.Host, DB.Port, DB.UserId, DB.Password, DB.DbName);
                    db = GetDmInstance(ConnectionString);
                    break;
                case "Kdbndp":
                    MessageBox.Show("Windows程序不支持连接到 人大金仓 数据库");
                    break;
            }
            //Print sql
            //db.Aop.OnLogExecuting = (sql, pars) =>
            //{
            //    Console.WriteLine(sql + "\r\n" + db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
            //    Console.WriteLine();
            //};
            //db.Aop.OnLogExecuted = (sql, pars) =>
            //{
            //    Console.WriteLine(sql + "\r\n" + db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
            //    Console.WriteLine();
            //};
            //db.Aop.OnError = (ex) =>
            //{
            //    Console.WriteLine(ex.ToString());
            //    Console.WriteLine();
            //};
            return db;
        }

        /// <summary>
        /// 创建 SqlServer 连接实例
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <returns></returns>
        private SqlSugarClient GetSqlServerInstance(string ConnectionString)
        {
            return new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = ConnectionString,
                DbType = SqlSugar.DbType.SqlServer,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }

        /// <summary>
        /// 创建 MySql 连接实例
        /// </summary>
        /// <returns></returns>
        private SqlSugarClient GetMySqlInstance(string ConnectionString)
        {
            return new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = ConnectionString,
                DbType = SqlSugar.DbType.MySql,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }

        /// <summary>
        /// Create SqlSugarClient
        /// </summary>
        /// <returns></returns>
        private SqlSugarClient GetOracleInstance(string ConnectionString)
        {
            return new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = ConnectionString,
                DbType = SqlSugar.DbType.Oracle,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }

        /// <summary>
        /// Create SqliteClient
        /// </summary>
        /// <returns></returns>
        private SqlSugarClient GetSqliteInstance(string ConnectionString)
        {
            return new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = ConnectionString,
                DbType = SqlSugar.DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }

        /// <summary>
        /// Create PostgreSQLClient
        /// </summary>
        /// <returns></returns>
        private SqlSugarClient GetPostgreSQLInstance(string ConnectionString)
        {
            return new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = ConnectionString,
                DbType = SqlSugar.DbType.PostgreSQL,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }

        /// <summary>
        /// 达梦
        /// </summary>
        /// <returns></returns>
        private SqlSugarClient GetDmInstance(string ConnectionString)
        {
            return new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = ConnectionString,
                DbType = SqlSugar.DbType.Dm,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }

        /// <summary>
        /// 人大金仓
        /// </summary>
        /// <returns></returns>
        private SqlSugarClient GetKdbndpInstance(string ConnectionString)
        {
            return new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = ConnectionString,
                DbType = SqlSugar.DbType.Kdbndp,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }

        #endregion

        int genFileCount = 0;
        private async void Button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;

            Split();
            Success("开始生成代码");


            string tempPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "template";

            templateFiles = GetTemplates(tempPath);
            genFileCount = 0;
            await Task.Run(() =>
            {
                // 遍历表列表
                foreach (var item in selectTableList)
                {
                    // 遍历模板列表
                    foreach (var file in templateFiles)
                    {
                        // 读取模板文件
                        ReadTemplate(file, item);
                    }
                }
            });

            Info("代码生成结束");
            Info("共" + genFileCount + "个文件");
            MessageBox.Show("生成操作完成");
            Process.Start(tempPath);
        }

        /// <summary>
        /// 模板集合
        /// </summary>
        private readonly Dictionary<string, string> tempDict = new Dictionary<string, string>();

        /// <summary>
        /// 根据路径读取模板
        /// </summary>
        /// <param name="path"></param>
        /// <param name="table"></param>
        private void ReadTemplate(FileInfo file, TableEntity table)
        {
            if (!File.Exists(file.FullName))
            {
                var msg = "模板" + file.Name + "不存在";
                MessageBox.Show(msg);
                Error(msg);
            }

            if (!tempDict.TryGetValue(file.FullName, out string CommandHandleTemplate))
            {
                CommandHandleTemplate = File.ReadAllText(file.FullName);
                tempDict.Add(file.FullName, CommandHandleTemplate);
            }

            string fileDir = file.DirectoryName;
            string fileName = file.Name.TrimEnd(file.Extension.ToCharArray());

            Render(ref CommandHandleTemplate, ref fileName, CreateModelParams(table));

            var outpath = Path.Combine(fileDir, "output");
            if (!Directory.Exists(outpath))
            {
                Directory.CreateDirectory(outpath);
                Success("生成目录 ：" + outpath);
            }

            var writePtah = Path.Combine(outpath, fileName);
            File.WriteAllText(writePtah, CommandHandleTemplate);

            Success("生成文件 ：" + writePtah);
            genFileCount++;
        }

        /// <summary>
        /// 解析模板生成代码文本
        /// </summary>
        /// <param name="templateStr">模板字符串</param>
        /// <param name="fileName">输出文件名称</param>
        /// <param name="params">模板参数</param>
        /// <returns></returns>
        public void Render(ref string templateStr, ref string fileName, Dictionary<string, string> param)
        {
            foreach (var item in param)
            {
                templateStr = templateStr.Replace(item.Key, item.Value);
                fileName = fileName.Replace(item.Key, item.Value);
            }
            fileName += ".cs";
        }

        /// <summary>
        /// 创建模板解析参数
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private Dictionary<string, string> CreateModelParams(TableEntity table)
        {
            Dictionary<string, string> tags = new Dictionary<string, string>
            {
                { "{{$GenInfo}}", GenInfo },//生成器版本信息
                { "{{$Company}}", Company },//公司名称
                { "{{$Department}}", Department },//部门名称

                { "{{$ModelName}}", ModelName },//模块名称
                { "{{$ApiVersion}}", ApiVersion },//接口版本整数            

                { "{{$TableName}}", table.TableName },//数据库表名
                { "{{$TableComment}}", table.TableComment },//表备注

                { "{{$EntityName}}", table.EntityName },//数据库实体类名 驼峰 大写开头 无前缀 无下划线
                { "{{$entityName}}", table.entityName },//数据库实体实例名 驼峰 大写开头 无前缀 无下划线
                { "{{$ENTITYNAME}}", table.ENTITYNAME },//全大写 无前缀 无下划线
                { "{{$entityname}}", table.entityname },//全小写 无前缀 无下划线

                { "{{$PreEntityName}}", table.PreEntityName },//驼峰 大写开头 有前缀 无下划线 适用类名
                { "{{$preEntityName}}", table.preEntityName },//驼峰 小写开头  有前缀 无下划线 适用类名
                { "{{$PREENTITYNAME}}", table.PREENTITYNAME },//全大写 有前缀 无下划线
                { "{{$preentityname}}", table.preentityname },//全小写 有前缀 无下划线

                //tags.Add("{{$columnName}}", "");//数据库列名
                //tags.Add("{{$columnComment}}", "");//列注释
                //tags.Add("{{$columnType}}", "");//列类型
                //tags.Add("{{$fieldType}}", "");//实体字段变量类型
                //tags.Add("{{$columnTypeLength}}", "");//列类型长度

                //tags.Add("{{$fieldName}}", "");//实体字段名称  驼峰 小写开头
                //tags.Add("{{$FieldName}}", "");//实体属性名称  驼峰 大写开头
                //tags.Add("{{$fieldname}}", "");//列名 全小写
                //tags.Add("{{$FIELDNAME}}", "");//列名 全大写

                { "{{$pkColumnName}}", table.pkColumnName },//数据库主键名称
                { "{{$pkColumnComment}}", table.pkColumnComment },//主键备注
                { "{{$pkColumnType}}", table.pkColumnType },//数据库主键类型
                { "{{$pkFieldType}}", table.pkFieldType },//实体主键变量类型
                { "{{$pkColumnTypeLength}}", table.pkColumnTypeLength.ToString() },//列类型长度

                { "{{$pkFieldName}}", table.pkFieldName },//主键字段名称  驼峰 小写开头
                { "{{$PkFieldName}}", table.PkFieldName },//主键属性名称  驼峰 大写开头
                { "{{$pkfieldname}}", table.pkfieldname },//主键名 全小写
                { "{{$PKFIELDNAME}}", table.PKFIELDNAME }//主键名 全大写
            };


            var datetime = DateTime.Now;
            tags.Add("{{$Data}}", datetime.ToShortDateString().ToString());//日期
            tags.Add("{{$Time}}", datetime.ToShortTimeString().ToString());//时间
            tags.Add("{{$DataTime}}", datetime.ToLocalTime().ToString());//日期时间

            return tags;
        }


        /// <summary>
        /// 获取指定目录下面的模板文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<FileInfo> GetTemplates(string path)
        {
            return FileSeacher.getFiles(path, "mtp");
        }

        private int tableSelectIndex = 0;
        private int RowIndex = 0;
        private DataGridViewCellMouseEventArgs e1;
        /// <summary>
        /// 表格单元格点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e == null) return;
            if (e.ColumnIndex == -1 || e.RowIndex == -1)
            {
                return;
            }
            button2.Enabled = false;
            tableSelectIndex = e.ColumnIndex;
            RowIndex = e.RowIndex;
            e1 = e;

            DataGridViewCell cell = dgvTableList.Rows[e.RowIndex].Cells[1];
            string tableName = cell.Value.ToString();
            if (tableStructsList.TryGetValue(tableName, out DataTable dt))
            {

                dgvTableStruce.DataSource = dt;
                label9.Text = tableName;
                label6.Text = dt.Rows.Count.ToString();
            }
            else
            {
                dgvTableStruce.DataSource = null;
                label9.Text = "";
                label6.Text = "0";
                return;
            }

            DataGridViewCheckBoxCell chckcell = (DataGridViewCheckBoxCell)dgvTableList.Rows[e.RowIndex].Cells[0];
            if (null == chckcell.Value || "False".Equals(chckcell.Value.ToString()))
            {
                chckcell.Value = true;
                selectCount++;
                chckcell.Style.BackColor = Color.Yellow;
                selectTableNameMap.Add(tableName, tableName);
            }
            else
            {
                chckcell.Value = false;
                selectCount--;
                chckcell.Style.BackColor = dgvTableList.Rows[e.RowIndex].Cells[1].Style.BackColor;
                selectTableNameMap.Remove(tableName);
            }

            lblTableSelectCount.Text = selectCount.ToString();
            btnGenCode.Enabled = selectCount > 0;
            button4.Enabled = btnGenCode.Enabled;

        }

        private List<TableEntity> selectTableList = new List<TableEntity>();
        private Dictionary<string, string> selectTableNameMap = new Dictionary<string, string>();
        /// <summary>
        /// 生成按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button6_Click(object sender, EventArgs e)
        {
            // 弹出对话框 提示勾选的表列表信息
            FrmCommon fg = new FrmCommon();
            fg.CallBack += SetCommon;
            fg.SetSelectCount(selectCount);
            var tableNames = selectTableNameMap.Keys.ToList();
            selectTableList = tableList.Where(c => tableNames.Contains(c.Key)).Select(o => o.Value).ToList();
            fg.ShowDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            // 打印表信息
            var tables = dbClient.DbMaintenance.GetTableInfoList();

            tables.ForEach(t =>
            {
                Console.WriteLine(t.Description);
                Console.WriteLine(t.Name);

                Console.WriteLine("*********  主键  ***********");
                var ids = dbClient.DbMaintenance.GetPrimaries(t.Name);
                if (ids != null && ids.Count > 0)
                {
                    Console.WriteLine("主键: " + ids[0]);
                }
                else
                {
                    Console.WriteLine("none primarie");
                }

                var cols = dbClient.DbMaintenance.GetColumnInfosByTableName(t.Name);
                Console.WriteLine("*********  列结构  ***********");
                cols.ForEach(c =>
                {

                    Console.WriteLine("列名：" + c.DbColumnName);
                    Console.WriteLine("列数据类型：" + c.DataType);
                    Console.WriteLine("列备注：" + c.ColumnDescription);
                    Console.WriteLine("列小数号：" + c.DecimalDigits);
                    Console.WriteLine("列默认：" + c.DefaultValue);

                    Console.WriteLine("Length：" + c.Length);
                    Console.WriteLine("IsPrimarykey：" + c.IsPrimarykey);
                    Console.WriteLine("IsIdentity：" + c.IsIdentity);
                    Console.WriteLine("PropertyName：" + c.PropertyName);
                    Console.WriteLine("PropertyType：" + c.PropertyType);

                    Console.WriteLine("Scale：" + c.Scale);
                    Console.WriteLine("TableId：" + c.TableId);
                    Console.WriteLine("Value：" + c.Value);
                    Console.WriteLine();
                });

            });

            var dbs = dbClient.DbMaintenance.GetDataBaseList(dbClient);
            Console.WriteLine("********************");

            dbs.ForEach(db =>
            {
                Console.WriteLine(db);

            });

            Console.WriteLine("********************");



        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn
            {
                Name = "select",
                //checkBoxColumn.HeaderText = "选 择";
                Width = 75
            };
            dgvTableList.Columns.Add(checkBoxColumn);
        }

        /// <summary>
        /// 拖动分割栏取消焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SplitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            button3.Focus();
        }

        /// <summary>
        /// 连接菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 增加新连接ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 打开连接对话框 读取配置文件 测试连通性 选择数据库类型 5种类型 输入数据库连接参数
            FrmConnection fc = new FrmConnection();

            if (DialogResult.Yes != fc.ShowDialog())
            {
                return;
            }

            // 获取填写的配置信息
            dbparam = fc.selectDbParm;



            if (dbClient != null)
            {
                dbClient.Close();
            }

            // 根据填写的配置 获取数据客户端
            dbClient = GetDBInstance(dbparam);

            if (dbClient == null)
            {
                Error("找不到连接对象，请检查填写的配置信息");
                return;
            }

            lblDbName.ForeColor = Color.Black;
            lblDbName.BackColor = label7.BackColor;

            lblDbType.ForeColor = Color.Black;
            lblDbType.BackColor = label7.BackColor;
            // 显示数据库类型
            lblDbType.Text = dbparam.DbType;
            // 显示数据库名称
            lblDbName.Text = dbparam.DbName;

            StringBuilder sb = new StringBuilder();
            sb.Append("Connection String ：");
            sb.Append(dbClient.Context.Ado.Connection.ConnectionString);

            Info(sb.ToString());

            // 获取数据库列表
            GetTableInfo();

        }


        #region 日志

        private void Split()
        {
            Success("—————————————————————————————————————————————————————");
        }


        private void Success(string msg)
        {
            Log("  [ √ ]  " + msg);
        }

        private void Info(string msg)
        {
            Log("  [ + ]  " + msg);
        }

        private void Error(string msg)
        {
            Log("  [ × ]  " + msg);
        }

        delegate void AppendValueDelegate(string msg);
        private void AddValueToTextBox(string msg)
        {
            _log.AppendText(msg + Environment.NewLine);
        }
        private void Log(string msg)
        {
            _log.BeginInvoke(new Action<string>(m => _log.AppendText(DateTime.Now.ToString() + m + Environment.NewLine)),
                new object[] { msg });
            //_log.BeginInvoke(new AppendValueDelegate(AddValueToTextBox), new object[] { msg });
        }

        #endregion

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmAbout about = new FrmAbout();
            about.ShowDialog();
        }

        private void 查看模板变量ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewMethod();
        }

        FrmShowInfo si = null;
        private void NewMethod()
        {
            if (dgvTableList.SelectedRows?.Count > 0)
            {
                string tableName = dgvTableList.SelectedRows[0].Cells[1].Value.ToString();
                if (tableList.TryGetValue(tableName, out var table))
                {
                    Info("查看表 " + tableName + " 表信息模板变量");
                    if (si == null)
                    {
                        si = new FrmShowInfo();
                    }
                    si.SetMsg(table.ToString());
                    si.ShowDialog();

                    //Console.WriteLine(table.ToString());
                }
                else
                {
                    MessageBox.Show("没找到表信息");
                }
            }
        }

        private void 查看模板变量ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            NewMethod1();
        }

        private void NewMethod1()
        {
            if (dgvTableList.SelectedRows?.Count > 0)
            {
                string tableName = dgvTableList.SelectedRows[0].Cells[1].Value.ToString();

                if (dgvTableStruce.SelectedRows?.Count > 0)
                {
                    string colname = dgvTableStruce.SelectedRows[0].Cells[0].Value.ToString();

                    if (tableList.TryGetValue(tableName, out var table))
                    {

                        Info("查看表 " + tableName + " 列 " + colname + " 信息模板变量");
                        var coldict = table.Cols;

                        if (coldict.TryGetValue(colname, out var col))
                        {
                            //Console.WriteLine(col.ToString());
                            if (si == null)
                            {
                                si = new FrmShowInfo();
                            }
                            si.SetMsg(col.ToString());
                            si.ShowDialog();
                        }
                        //var cols = coldict.Values.ToList();

                        //foreach (var col in cols)
                        //{

                        //}
                    }
                    else
                    {
                        MessageBox.Show("没找到列信息");
                    }
                }

            }
        }


        private void 清空ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _log.Text = "";
        }

        private void 连接数据库ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {
            button3.Focus();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            int count = dgvTableList.Rows.Count;

            if (count > 0)
            {
                if (checkBox1.Checked)
                {
                    for (int i = 0; i < count; i++)
                    {
                        DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)dgvTableList.Rows[i].Cells[0];
                        cell.Value = true;
                        cell.Style.BackColor = Color.Yellow;
                    }
                    checkBox1.Text = "取消";
                    btnGenCode.Enabled = true;
                    button4.Enabled = true;
                    selectCount = count;
                    selectTableNameMap = tableList.ToDictionary(c => c.Key, c => c.Key);
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)dgvTableList.Rows[i].Cells[0];
                        cell.Value = false;
                        cell.Style.BackColor = dgvTableList.Rows[i].Cells[1].Style.BackColor;
                    }
                    checkBox1.Text = "全选";
                    btnGenCode.Enabled = false;
                    button4.Enabled = false;
                    selectCount = 0;
                    selectTableNameMap.Clear();
                }
                lblTableSelectCount.Text = selectCount.ToString();
            }
        }

        private void dgvTableList_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void dgvTableList_KeyUp(object sender, KeyEventArgs e)
        {
            if (dgvTableList.SelectedRows.Count > 0 && e1 != null)
            {
                RowIndex = dgvTableList.SelectedRows[0].Index;
                tableSelectIndex = 0;
                DataGridView1_CellMouseClick(null, new DataGridViewCellMouseEventArgs(tableSelectIndex, RowIndex, 0, 0, e1));
            }

        }


        private async void button4_Click(object sender, EventArgs e)
        {
            Split();
            Info("开始生成实体");
            await Task.Run(() =>
            {
                foreach (var item in dbClient.DbMaintenance.GetTableInfoList())
                {
                    string entityName = StringUtils.ToEntityName(item.Name);/*实体名大写*/
                    dbClient.MappingTables.Add(entityName, item.Name);
                    foreach (var col in dbClient.DbMaintenance.GetColumnInfosByTableName(item.Name))
                    {
                        dbClient.MappingColumns.Add(StringUtils.ToEntityName(col.DbColumnName) /*类的属性开头大写*/, col.DbColumnName, entityName);
                    }
                }
                var entityPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "DBEntitys";
                dbClient.DbFirst.IsCreateAttribute().Where(it => selectTableNameMap.Values.ToList().Contains(it)).CreateClassFile(entityPath, ModelName + ".Entities");


                Split();
                Info("实体生成完成");
                Process.Start(entityPath);
            });
        }


        /// <summary>
        /// 反选表列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                int count = dgvTableList.Rows.Count;

                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)dgvTableList.Rows[i].Cells[0];
                        DataGridViewCell cellName = dgvTableList.Rows[i].Cells[1];
                        var name = cellName.Value.ToString();
                        if (null == cell.Value)
                        {
                            cell.Value = true;
                        }
                        else
                        {
                            cell.Value = !(bool)(cell.Value);
                        }

                        if (cell.Style.BackColor == Color.Yellow)
                        {

                            selectTableNameMap.Remove(name);
                            cell.Style.BackColor = dgvTableList.Rows[i].Cells[1].Style.BackColor;
                        }
                        else
                        {
                            cell.Style.BackColor = Color.Yellow;
                            selectTableNameMap.Add(name, Name);
                        }
                    }

                    selectCount = selectTableNameMap.Count;
                    lblTableSelectCount.Text = selectCount.ToString();
                    btnGenCode.Enabled = selectCount > 0;
                    button4.Enabled = btnGenCode.Enabled;
                }
            }
            catch (Exception)
            {
            }

        }
    }
}
