/*
 * 说明：此类封装图片处理相关的接口，方便后续工程中直接直接使用
 * 作者：huangjun
 * 时间：2020-12-02
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaslerCameraCtrl.UserMethodClass
{
    class FileProcessClass
    {
        /// <summary>
        /// 获取指定文件夹下所有的文件
        /// </summary>
        /// <param name="fileName">文件夹名称</param>
        public void GetAllFilesInDictionary(string fileName)
        {
            DirectoryInfo dirs = new DirectoryInfo(System.Environment.CurrentDirectory + fileName);
            FileInfo[] file = dirs.GetFiles();// 获得目录下文件     
            // 循环文件
            for (int j = 0; j < file.Count(); j++) {
                if (file[j].Extension == ".txt") {
                    //带后缀
                    // 界面显示处理
                    // comboBox1.Items.Add(file[j].Name);
                    //获取目录或文件的完整目录。
                    // comboBox1.Items.Add(file[j].FullName);
                }
            }
        }

        /// <summary>
        /// 向dat文件中写入内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        public void WriteDatFile(string filePath,string data)
        {
            // 使用“另存为”对话框中输入的文件名实例化FileStream对象
            FileStream myStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            // 使用FileStream对象实例化BinaryWriter二进制写入流对象
            BinaryWriter myWriter = new BinaryWriter(myStream);
            // 以二进制方式向创建的文件中写入内容
            myWriter.Write(data);
            // 关闭当前二进制写入流
            myWriter.Close();
            // 关闭当前文件流
            myStream.Close();
        }

        /// <summary>
        /// 读取dat文件中的内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string ReadDatFile(string filePath)
        {
            string ret = string.Empty;
            FileStream myStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            // 使用FileStream对象实例化BinaryReader二进制写入流对象
            BinaryReader myReader = new BinaryReader(myStream);
            if (myReader.PeekChar() != -1) {
                // 以二进制方式读取文件中的内容
                ret = Convert.ToString(myReader.ReadString());
                return ret;
            }
            // 关闭当前二进制读取流
            myReader.Close();
            // 关闭当前文件流
            myStream.Close();
            return ret;
        }

        /// <summary>
        /// C#记录程序的运行日志
        /// </summary>
        /// <param name="msg"></param>
        public void WriteProcessLog(string msg)
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "Log";
            if (!Directory.Exists(filePath)) {
                Directory.CreateDirectory(filePath);
            }
            string logPath = AppDomain.CurrentDomain.BaseDirectory + "Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            try {
                using (StreamWriter sw = File.AppendText(logPath)) {
                    sw.WriteLine("时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    sw.WriteLine("消息：" + msg);
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                }
            } catch (IOException e) {
                using (StreamWriter sw = File.AppendText(logPath)) {
                    sw.WriteLine("时间：" + DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"));
                    sw.WriteLine("异常：" + e.Message + "\r\n" + e.StackTrace);
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                }
            }
        }







    }
}
