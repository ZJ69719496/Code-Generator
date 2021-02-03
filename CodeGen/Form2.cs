using System;
using System.IO;
using System.Windows.Forms;
using Commons.Collections;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime;


namespace CodeGen
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            VelocityEngine velocityEngine = new VelocityEngine();
            ExtendedProperties props = new ExtendedProperties();
            props.AddProperty(RuntimeConstants.RESOURCE_LOADER, "file");
            //props.AddProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, HttpContext.Current.Server.MapPath(templatDir);
            props.AddProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, @"");
            //props.AddProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, Path.GetDirectoryName(HttpContext.Current.Request.PhysicalPath));
            props.AddProperty(RuntimeConstants.INPUT_ENCODING, "utf-8");
            props.AddProperty(RuntimeConstants.OUTPUT_ENCODING, "utf-8");
            //模板的缓存设置
            props.AddProperty(RuntimeConstants.FILE_RESOURCE_LOADER_CACHE, true);              //是否缓存
            props.AddProperty("file.resource.loader.modificationCheckInterval", (Int64)30);    //缓存时间(秒)
            velocityEngine.Init(props);

            //为模板变量赋值
            VelocityContext context = new VelocityContext();
            context.Put("Time", "20170613");
            context.Put("Title", "模板生成");
            context.Put("Body", "内容");

            //  Template template = velocityEngine.GetTemplate(@"D:\ProgrammingFolder\C#\NVelocityDemo\NVelocityDemo\bin\Debug\Template");
            //从文件中读取模板
            Template template = velocityEngine.GetTemplate(@"C:\Users\Macroinf—PC135\source\repos\CodeGen\CodeGen\bin\Debug\Value.vm");
            //合并模板
            using (StreamWriter writer = new StreamWriter(@"C:\Users\Macroinf—PC135\source\repos\CodeGen\CodeGen\bin\Debug\123.cs", false))
            {
                template.Merge(context, writer);
            }
        }
    }
}
