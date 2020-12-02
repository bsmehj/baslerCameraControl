/*
 * 说明：此类对ini文件操作进行相关封装，对INI文件进行读取，方便后续工程中直接直接使用
 * 作者：huangjun
 * 时间：2020-12-03
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BaslerCameraCtrl.UserMethodClass
{
    //  <summary> 
    ///  引入命名空间 using System.Runtime.InteropServices;
    ///  读写ini文件的类 
    ///  调用kernel32.dll中的两个api：WritePrivateProfileString，
    ///                               GetPrivateProfileString来实现对ini  文件的读写。 
    ///  INI文件是文本文件, 
    ///  由若干节(section)组成, 
    ///  在每个带括号的标题下面, 
    ///  是若干个关键词(key)及其对应的值(value) 
    /// [Section] 
    /// Key=value 
    ///  </summary> 
    class IniFileOperateClass
    {
        private string path = string.Empty;

        public IniFileOperateClass(string inipath)
        {
            this.path = inipath;
        }

        // 引入动态库
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, 
            string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, 
            StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, 
            string defVal, Byte[] retVal, int size, string filePath);

        /// <summary>
        ///  向ini文件中写入字段值
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void WriteIniValue(string section, string key, string val)
        {
            WritePrivateProfileString(section, key, val, this.path);
        }


        /// <summary>
        /// 读取ini文件字段值
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string ReadIniValue(string section, string key)
        {
            StringBuilder strBuilder = new StringBuilder(255);
            int val = GetPrivateProfileString(section, key, "", strBuilder, 255, this.path);
            return strBuilder.ToString();
        }

        /// <summary>
        /// 读取ini文件字段值
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] ReadIniBytesValue(string section, string key)
        {
            byte[] val = new byte[255];
            int ret = GetPrivateProfileString(section, key, "", val, 255, this.path);
            return val;
        }

        /// <summary>
        /// 清除ini文件中字段下所有的字段，要谨慎使用
        /// </summary>
        public void ClearAllSectionVal()
        {
            WriteIniValue(null, null, null);
        }

        /// <summary>
        /// 清除ini文件中指定段落的键值
        /// </summary>
        /// <param name="section"></param>
        public void ClearAssignSectionVal(string section)
        {
            WriteIniValue(section, null, null);
        }




    }
}
