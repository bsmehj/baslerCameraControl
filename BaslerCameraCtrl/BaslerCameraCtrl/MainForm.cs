using BaslerCameraCtrl.BaslerCamera;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using BaslerCameraCtrl.UserMethodClass;



namespace BaslerCameraCtrl
{
    public partial class MainForm : Form
    {
        // 数据结构
        BaslerCameraDoNet myCamera = new BaslerCameraDoNet();


        public MainForm()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            myCamera.CameraImageEvent += MainForm_CameraImageEvent;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            int cameraNum = myCamera.GetCameraNum();
            if (cameraNum <= 0) {
                MessageBox.Show("未检测到相机，请检查！", "系统提示",
                    MessageBoxButtons.OKCancel);
            }

            //if (myCamera.CameraNumber > 0) {
            //    myCamera.CameraInit();
            //} else {
            //    MessageBox.Show("未检测到相机，请检查！", "系统提示",
            //        MessageBoxButtons.OKCancel);
            //}
        }

        private void MainForm_CameraImageEvent(Bitmap bmp)
        {
            try
            {
                pictureBox_basler.BeginInvoke(new MethodInvoker(delegate {
                    Bitmap oldBmp = pictureBox_basler.Image as Bitmap;
                    pictureBox_basler.Image = bmp;
                    if (oldBmp != null) {
                        oldBmp.Dispose();
                    }
                }));
            }
            catch (System.Exception ex)
            {
                string err = ex.Message;
            }
            
        }
        
        private void button_open_camera_Click(object sender, EventArgs e)
        {
            int cameraNum = myCamera.GetCameraNum();
            if (cameraNum > 0) {
                if (myCamera.IsCameraOpen()) {
                    // 界面设置
                    // todo
                    return;
                }
                myCamera.CameraInit();
                if (myCamera.IsCameraOpen()) {
                    // 界面设置
                    button_open_camera.Enabled = false;
                    button_close_camera.Enabled = true;
                    button_pause_grab.Enabled = true;
                    button_single_grab.Enabled = true;
                    button_continue_grab.Enabled = true;
                }
            } else {
                MessageBox.Show("未检测到相机，请检查！", "系统提示", 
                    MessageBoxButtons.OKCancel);
            }
        }

        // 关闭相机
        private void button_close_camera_Click(object sender, EventArgs e)
        {
            try
            {
                myCamera.DestoryCamera();
                // 界面设置
                button_open_camera.Enabled = true;
                button_close_camera.Enabled = false;
                button_pause_grab.Enabled = false;
                button_single_grab.Enabled = false;
                button_continue_grab.Enabled = false;

            }
            catch (System.Exception ex)
            {
                string err = ex.Message;
            }
        }

        private void button_single_grab_Click(object sender, EventArgs e)
        {
            myCamera.SingleGrab();
        }

        private void button_continue_grab_Click(object sender, EventArgs e)
        {
            myCamera.ContinueGrab();

            // 注意：图像连续采集过程中将占用大量内存，后续该功能可能放在线程中进行比较好
            // 界面设置
            button_pause_grab.Enabled = true;
            button_single_grab.Enabled = false;
            button_continue_grab.Enabled = false;
        }

        // 暂停采集
        private void button_pause_grab_Click(object sender, EventArgs e)
        {
            myCamera.StopGrab();

            // 界面设置
            // 暂停按钮按下之后，摄像头将停止图像采集，此时需要再次进行图像采集的时候
            // 需要重新设定获取图像的采集模式
            button_pause_grab.Enabled = false;
            button_single_grab.Enabled = true;
            button_continue_grab.Enabled = true;
        }

        // 关闭主界面
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            myCamera.DestoryCamera();
            Process.GetCurrentProcess().Kill();  // 程序退出最好的办法
        }

        


    }
}
