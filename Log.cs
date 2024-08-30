using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewsFinder
{
    public static class Log
    {
        public static string folderPath = Application.StartupPath + @"\Log";
        public static string filePath = folderPath + @"\log_" + DateTime.Today.ToString("yyyyMMdd") + ".log";


        public static DirectoryInfo di = new DirectoryInfo(folderPath);
        public static FileInfo fi = new FileInfo(filePath);
        public static string temp;

        public static void LogCreate()
        {
            if (!di.Exists)
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!fi.Exists)
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    temp = string.Format("{0} 로그 파일 최초 생성", DateTime.Now);
                    sw.WriteLine(temp);
                    sw.Close();
                }
            }

        }

        public static void LogWrite(string msg)
        {
            using (StreamWriter sw = File.AppendText(filePath))
            {
                temp = string.Format("{0} {1}", DateTime.Now, msg);
                sw.WriteLine(temp);
                sw.Close();
            }
        }

       
    }
}
