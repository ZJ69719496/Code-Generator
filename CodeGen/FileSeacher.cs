using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGen
{
    public partial class FileSeacher
    {
        /// <summary>
        /// 私有变量
        /// </summary>
        private static List<FileInfo> lst = new List<FileInfo>();

        /// <summary>
        /// 获得目录下所有文件或指定文件类型文件(包含所有子文件夹)
        /// </summary>
        /// <param name="path">文件夹路径</param>
        /// <param name="Extension">扩展名可以多个 例如 .mp3.wma.rm</param>
        /// <returns>List<FileInfo></returns>
        public static List<FileInfo> getFiles(string path, string Extension)
        {
            getFilesInDir(path, "." + Extension);
            return lst;
        }

        /// <summary>
        /// 私有方法,递归获取指定类型文件,包含子文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <param name="Extension"></param>
        private static void getFilesInDir(string path, string Extension)
        {
            try
            {
                string[] dir = Directory.GetDirectories(path); //文件夹列表
                DirectoryInfo fdir = new DirectoryInfo(path);
                FileInfo[] file = fdir.GetFiles();
                if (file.Length != 0 || dir.Length != 0) //当前目录文件或文件夹不为空
                {
                    foreach (FileInfo f in file) //显示当前目录所有文件
                    {
                        if (Extension.ToLower().Equals(f.Extension.ToLower()))
                        {
                            lst.Add(f);
                        }
                    }

                    foreach (string d in dir)
                    {
                        getFilesInDir(d, Extension);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
