using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Basler.Pylon; // 第三方库
using System.Drawing;
using System.Drawing.Imaging;

namespace BaslerCameraCtrl.BaslerCamera
{
    class BaslerCameraDoNet
    {
        // 获取当前电脑连接相机的个数
        public int CameraNumber = CameraFinder.Enumerate().Count;
        public int GetCameraNum()
        {
            return CameraFinder.Enumerate().Count;
        }

        // 单次采集图片的张数
        const uint SINGLE_IMAGE_NUM = 1;

        //委托+事件 = 回调函数，用于传递相机抓取的图像
        public delegate void CameraImage(Bitmap bmp);
        public event CameraImage CameraImageEvent;

        //放出一个Camera
        private Camera camera = null;

        //basler里用于将相机采集的图像转换成位图
        PixelDataConverter pxConvert = new PixelDataConverter();

        //控制相机采集图像的过程
        bool GrabOver = false;

        // 测量图像采集的时间
        private Stopwatch stopwatch = new Stopwatch();

        public void CameraInit()
        {
            camera = new Camera();
            //自由运行模式
            camera.CameraOpened += Configuration.AcquireContinuous;

            //断开连接事件
            camera.ConnectionLost += CameraConnectionLost;

            //相机抓取开始事件
            camera.StreamGrabber.GrabStarted += StreamGrabberGrabStarted;

            //抓取图片事件
            camera.StreamGrabber.ImageGrabbed += StreamGrabberImageGrabbed;

            //结束抓取事件
            camera.StreamGrabber.GrabStopped += StreamGrabberGrabStopped;

            //打开相机
            camera.Open();
        }

        public bool IsCameraOpen()
        {
            if (camera != null) {
                return camera.IsOpen;
            }
            return false;
        }

        // 相机断开连接事件
        private void CameraConnectionLost(object sender, EventArgs e)
        {
            camera.StreamGrabber.Stop();
            DestoryCamera();
        }

        // 相机抓取开始事件
        private void StreamGrabberGrabStarted(object sender, EventArgs e)
        {
            GrabOver = true;
        }

        private void StreamGrabberGrabStopped(object sender, GrabStopEventArgs e)
        {
            GrabOver = false;
        }

        private void StreamGrabberImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            IGrabResult grabResult = e.GrabResult;
            if (grabResult.IsValid) {
                if (GrabOver) {
                    CameraImageEvent(GrabResult2Bmp(grabResult));
                }
            }
        }

        public void DestoryCamera()
        {
            if (camera != null) {
                camera.Close();
                camera.Dispose();
                camera = null;
            }
        }

        // 将相机采集的图像转换为bmp格式图片
        Bitmap GrabResult2Bmp(IGrabResult grabResult)
        {
            Bitmap bmp = new Bitmap(grabResult.Width, grabResult.Height, PixelFormat.Format32bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), 
                ImageLockMode.ReadWrite, bmp.PixelFormat);
            pxConvert.OutputPixelFormat = PixelType.BGRA8packed;
            IntPtr bmpIntpr = bmpData.Scan0;
            pxConvert.Convert(bmpIntpr, bmpData.Stride * bmp.Height, grabResult);
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        // 单次采集
        public void SingleGrab()
        {
            if (camera != null) {
                camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                camera.StreamGrabber.Start(SINGLE_IMAGE_NUM, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
        }

        // 持续采集
        public void ContinueGrab()
        {
            if (camera != null) {
                camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
        }

        public void StopGrab()
        {
            if (camera != null) {
                camera.StreamGrabber.Stop();
            }
        }


        #region 相机设置

        // 设置相机的软触发模式
        public bool SetCameraSoftTrigger()
        {
            try {
                camera.ExecuteSoftwareTrigger();
            } catch (System.Exception ex) {
                string err = ex.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 设置曝光时间us
        /// </summary>
        /// <param name="exposureTime"></param>
        /// <returns></returns>
        public bool SetExposureTime(long exposureTime)
        {
            try {
                camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(exposureTime);
            } catch (System.Exception ex) {
                string err = ex.Message;
                return false;

            }
            return true;
        }

        // 
        /// <summary>
        /// 设置相机图像格式
        /// </summary>
        /// <param name="pixelType"></param>
        /// <returns></returns>
        public bool SetPixelFormat(uint pixelType)//设置图像格式
        {
            // 1：Mono8  2：彩色YUV422
            try
            {
                if (pixelType == 1) {
                    camera.Parameters[PLCamera.PixelFormat].TrySetValue(PLCamera.PixelFormat.Mono8);
                } else if (pixelType == 2) {
                    camera.Parameters[PLCamera.PixelFormat].TrySetValue(PLCamera.PixelFormat.YUV422Packed);
                }
            }
            catch (System.Exception ex)
            {
                string err = ex.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 设置图像高度
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public bool SetImgHeight(long height)
        {
            try
            {
                if (camera.Parameters[PLCamera.Height].TrySetValue(height)) {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                string err = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 设置图像宽度
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        public bool SetImgWidth(long width)
        {
            try
            {
                if (camera.Parameters[PLCamera.Width].TrySetValue(width)) {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                string err = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 设置图像水平偏移
        /// </summary>
        /// <param name="offsetX"></param>
        /// <returns></returns>
        public bool SetImgOffsetX(long offsetX)
        {
            try
            {
                if (camera.Parameters[PLCamera.OffsetX].TrySetValue(offsetX)) {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                string err = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 设置图像竖直偏移
        /// </summary>
        /// <param name="offsetY"></param>
        /// <returns></returns>
        public bool SetImgOffsetY(long offsetY)
        {
            try
            {
                if (camera.Parameters[PLCamera.OffsetY].TrySetValue(offsetY)) {
                    return true;
                }
                return false;
            }
            catch (System.Exception ex)
            {
                string err = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 设置相机自动白平衡
        /// </summary>
        /// <param name="balanceVal">off:关闭；on:打开</param>
        /// <returns></returns>
        public bool SetCameraBalanceAuto(string balanceVal)
        {
            try
            {
                camera.Parameters[PLCamera.BalanceWhiteAuto].TrySetValue(balanceVal);
            }
            catch (System.Exception ex)
            {
                string err = ex.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断图像是否为黑白格式
        /// </summary>
        /// <param name="iGrabResult"></param>
        /// <returns></returns>
        public bool IsMonoData(IGrabResult iGrabResult)
        {
            switch (iGrabResult.PixelTypeValue)
            {
                case PixelType.Mono1packed:
                case PixelType.Mono2packed:
                case PixelType.Mono4packed:
                case PixelType.Mono8:
                case PixelType.Mono8signed:
                case PixelType.Mono10:
                case PixelType.Mono10p:
                case PixelType.Mono10packed:
                case PixelType.Mono12:
                case PixelType.Mono12p:
                case PixelType.Mono12packed:
                case PixelType.Mono16:
                    return true;
                default:
                    return false;
            }
        }


        #endregion


    }
}
