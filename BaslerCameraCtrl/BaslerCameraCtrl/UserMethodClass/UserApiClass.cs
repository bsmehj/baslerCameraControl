using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace BaslerCameraCtrl.UserMethodClass
{
    class UserApiClass
    {
        /// <summary>
        /// 检测指定目录是否存在
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public bool IsDirectoryExist(string dirPath)
        {
            return Directory.Exists(dirPath);
        }

        /// <summary>
        /// 获取选择文件的路劲
        /// </summary>
        /// <returns></returns>
        public string GetSelectPath()
        {
            string path = string.Empty;
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                path = fbd.SelectedPath;
            }
            return path;
        }

        /// <summary>
        /// 获取程序的基目录
        /// </summary>
        /// <returns></returns>
        public string GetProcBasePath()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 获取模块的完整目录包括文件名
        /// </summary>
        /// <returns></returns>
        public string GetMainModulePathAndFileName()
        {
            return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        }

        /// <summary>
        /// 获取和设置当前目录(该进程从中启动的目录)的完全限定目录。
        /// </summary>
        /// <returns></returns>
        public string GetCurDirectPath()
        {
            return System.Environment.CurrentDirectory;
        }

        /// <summary>
        /// 获取应用程序的当前工作目录，注意工作目录是可以改变的，而不限定在程序所在目录。
        /// </summary>
        /// <returns></returns>
        public string GetPrjCurWorkPath()
        {
            return System.IO.Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// 获取和设置包括该应用程序的目录的名称。
        /// </summary>
        /// <returns></returns>
        public string GetCurDomainPath()
        {
            return System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        }

        /// <summary>
        /// 获取启动了应用程序的可执行文件的路径。
        /// </summary>
        /// <returns></returns>
        public string GetStartAppPath()
        {
            return System.Windows.Forms.Application.StartupPath;
        }

        /// <summary>
        /// 获取启动了应用程序的可执行文件的路径及文件名
        /// </summary>
        /// <returns></returns>
        public string GetStartAppPathAndName()
        {
            return System.Windows.Forms.Application.ExecutablePath;
        }

        /// <summary>
        /// 获取电脑硬盘的编号
        /// </summary>
        /// <returns></returns>
        public string GetDiskVolumeSerialNumber()
        {
            ManagementClass processClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
            disk.Get();
            return disk.GetPropertyValue("VolumeSerialNumber").ToString();
        }

        /// <summary>
        /// 获取电脑CPU的序列号
        /// 注意：需要在引用中添加management的程序集引用
        /// </summary>
        /// <returns></returns>
        public string GetCpuId()
        {
            string cpuId = string.Empty;
            ManagementClass mc = new ManagementClass("win32_Processor");
            ManagementObjectCollection cpuCollection = mc.GetInstances();
            foreach (ManagementObject obj in cpuCollection) {
                cpuId = obj.Properties["Processorid"].Value.ToString();
                break;
            }
            return cpuId;
        }

        /// <summary>
        /// 通过CPUID和硬盘序列号生成机器码
        /// </summary>
        /// <returns></returns>
        public string GetMachineNum()
        {
            string machineNum = string.Empty;
            string str = GetCpuId() + GetDiskVolumeSerialNumber(); // 获得24位Cpu和硬盘序列号
            machineNum = str.Substring(0, 24); // 从生成的字符串中取出前24个字符做为机器码
            return machineNum;
        }

        private int[] intCode = new int[127];//存储密钥
        private int[] intNumber = new int[25];//存机器码的Ascii值
        private char[] charCode = new char[25];//存储机器码字
        /// <summary>
        /// 给数组赋值小于10的数
        /// </summary>
        public void SetIntCode()
        {
            for (int i = 1; i < intCode.Length; i++)
            {
                intCode[i] = i % 9;
            }
        }

        // 生成注册码
        public string GetRegisterNum()
        {
            SetIntCode();//初始化127位数组
            string machineNum = this.GetMachineNum();//获取注册码
            for (int i = 1; i < charCode.Length; i++) {
                // 把机器码存入数组中
                charCode[i] = Convert.ToChar(machineNum.Substring(i - 1, 1));
            }
            for (int j = 1; j < intNumber.Length; j++) {
                //把字符的ASCII值存入一个整数组中
                intNumber[j] = intCode[Convert.ToInt32(charCode[j])] + Convert.ToInt32(charCode[j]);
            }
            string strAsciiName = "";//用于存储注册码
            for (int j = 1; j < intNumber.Length; j++) {
                if (intNumber[j] >= 48 && intNumber[j] <= 57) {
                    //判断字符ASCII值是否0－9之间
                    strAsciiName += Convert.ToChar(intNumber[j]).ToString();
                } else if (intNumber[j] >= 65 && intNumber[j] <= 90) {
                    //判断字符ASCII值是否A－Z之间
                    strAsciiName += Convert.ToChar(intNumber[j]).ToString();
                } else if (intNumber[j] >= 97 && intNumber[j] <= 122) {
                    //判断字符ASCII值是否a－z之间
                    strAsciiName += Convert.ToChar(intNumber[j]).ToString();
                }  else {
                    //判断字符ASCII值不在以上范围内
                    if (intNumber[j] > 122) {
                        //判断字符ASCII值是否大于z
                        strAsciiName += Convert.ToChar(intNumber[j] - 10).ToString();
                    } else {
                        strAsciiName += Convert.ToChar(intNumber[j] - 9).ToString();
                    }
                }
            }
            return strAsciiName;//返回注册码
        }

        /// <summary>
        /// 向文本文件尾部添加内容
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public void AppendText(string path, string content)
        {
            File.AppendAllText(path, content);
        }

        /// <summary>
        /// 在TXT文本中追加内容
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public void AppendTxtContent(string path, string content)
        {
            FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate);
            StreamWriter writeStream = new StreamWriter(fileStream);
            writeStream.Write(content);
            writeStream.Close();
            fileStream.Close();
        }

        /// <summary>
        /// 在当前日期的基础上将天数向前或者向后推移
        /// </summary>
        /// <param name="days">该值设置为正数表示向后增加天数，设置为负数表示减天数</param>
        /// <returns></returns>
        public string AddDayOnCurDate(double days)
        {
            DateTime dt = DateTime.Now;
            string addDays = dt.AddDays(days).ToString("yyyyMMddHHmmss");
            return addDays;
        }

        /// <summary>
        /// 获取指定驱动器的空间总大小(单位为GB) 
        /// </summary>
        /// <param name="str_HardDiskName">只需输入代表驱动器的字母即可</param>
        /// <returns></returns>
        public static long GetHardDiskSpace(string str_HardDiskName)
        {
            long totalSize = new long();
            str_HardDiskName = str_HardDiskName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives) {
                if (drive.Name == str_HardDiskName) {
                    totalSize = drive.TotalSize / (1024 * 1024 * 1024);
                }
            }
            return totalSize;
        }

        /// <summary>
        /// 获取指定驱动器的剩余空间总大小(单位为GB)
        /// </summary>
        /// <param name="str_HardDiskName">只需输入代表驱动器的字母即可</param>
        /// <returns></returns>
        public static long GetHardDiskFreeSpace(string str_HardDiskName)
        {
            long freeSpace = new long();
            str_HardDiskName = str_HardDiskName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives) {
                if (drive.Name == str_HardDiskName) {
                    freeSpace = drive.TotalFreeSpace / (1024 * 1024 * 1024);
                }
            }
            return freeSpace;
        }

        /// <summary>
        /// C#截取屏幕并保存
        /// </summary>
        public void GetPrintScreen()
        {
            try {
                // 获取屏幕宽 
                int iWidth = Screen.PrimaryScreen.Bounds.Width;
                // 获取屏幕高
                int iHeight = Screen.PrimaryScreen.Bounds.Height;
                // 按照屏幕宽高创建位图
                Image img = new Bitmap(iWidth, iHeight);
                // 从一个继承自Image类的对象中创建Graphics对象
                Graphics gc = Graphics.FromImage(img);
                // 抓屏并拷贝到image里
                gc.CopyFromScreen(new System.Drawing.Point(0, 0), new System.Drawing.Point(0, 0), new Size(iWidth, iHeight));
                // this.BackgroundImage = img;  // 设置窗体的背景照片
                // 保存
                string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
                System.Windows.Forms.SaveFileDialog dig_saveImage = new System.Windows.Forms.SaveFileDialog();
                dig_saveImage.Title = "请选择图像保存路径";
                dig_saveImage.Filter = "Image File(*.png)|Image File|*.tif|*.png|Image File(*.jpg)|*.jpg|Image File(*.bmp)|*.bmp";
                dig_saveImage.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                dig_saveImage.FileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                if (dig_saveImage.ShowDialog() == DialogResult.OK) {
                    img.Save(dig_saveImage.FileName);       //保存位图
                }
            } catch (System.Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// C#截取屏幕并返回图片
        /// </summary>
        /// <returns>Image</returns>
        public Image GetScreenImg()
        {
            Image myImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(myImage);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), 
                new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
            IntPtr dc1 = g.GetHdc();
            g.ReleaseHdc(dc1);
            //String path = "d:\\image\\";
            // if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            Random objRand = new Random();
            String pic_name = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".jpg";
            //string allpathname = path + pic_name;
            //myImage.Save(allpathname);
            return myImage;

        }

        /// <summary>
        /// 将图片转换为二进制流
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public byte[] GetScreenStreamToBytes(Bitmap bmp)
        {
            System.IO.MemoryStream s = new System.IO.MemoryStream();
            bmp.Save(s, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] Data = s.ToArray();
            s.Close();
            s.Dispose();
            return Data;
        }

        






















        }
    }
