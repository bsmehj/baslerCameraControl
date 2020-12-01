/*
 * 说明：此程序是基于Pylon6版本的开发库basler.pylon.dll所写，支持黑白/彩色的USB3.0工业相机。
 *       对相机进行相关操作
 * 作者：huangjun
 * 时间：2020-11-25
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Basler.Pylon; // 第三方库

namespace BaslerCameraCtrl.BaslerCamera
{
    class BaslerCameraClass
    {
        private Camera baslerCamera = null;
        private PixelDataConverter converter = new PixelDataConverter();
        public String strModelName = null;
        public String strSerialNumber = null;
        public String strUserID = null;

        public int imageWidth = 0;          // 图像宽
        public int imageHeight = 0;         // 图像高
        public long payloadSize = 0;        // 图像大小
        public long minExposureTime = 0;    // 最小曝光时间
        public long maxExposureTime = 0;    // 最大曝光时间
        public long minGain = 0;            // 最小增益
        public long maxGain = 0;            // 最大增益
        public int numWindowIndex = 0;      // pylon自带窗体编号

        private long grabTime = 0;                          // 采集图像时间
        private Boolean isColor = false;                    // 判断是否是彩色图像
        private IntPtr latestFrameAddress = IntPtr.Zero;    // 图像格式转换后的首地址，用于pylon转halcon,visionpro等图像变量
        private Stopwatch stopWatch = new Stopwatch();

        /// <summary>
        /// 计算采集图像时间自定义委托
        /// </summary>
        /// <param name="time">采集图像时间</param>
        public delegate void delegateComputeGrabTime(long time);

        /// <summary>
        /// 计算采集图像时间委托事件
        /// </summary>
        public event delegateComputeGrabTime eventComputeGrabTime;

        /// <summary>               
        /// 图像处理自定义委托
        /// </summary>
        public delegate void delegateProcessHImage(Boolean isColor, 
            int width, int height, IntPtr frameAddress);

        /// <summary>
        /// 图像处理委托事件
        /// </summary>
        public event delegateProcessHImage eventProcessImage;

        /// <summary>
        /// if >= Sfnc2_0_0,说明是ＵＳＢ３的相机
        /// </summary>
        static Version Sfnc2_0_0 = new Version(2, 0, 0);

        /// <summary>
        /// 异常处理日志
        /// </summary>
        /// <param name="exception"></param>
        private void ShowException(Exception exception)
        {
            MessageBox.Show("Exception caught:\n" + exception.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 实例化第一个找到的相机
        /// </summary>
        public BaslerCameraClass()
        {
            try {
                baslerCamera = new Camera(CameraSelectionStrategy.FirstFound);
            }
            catch (Exception e) {
                ShowException(e);
            }
        }

        /// <summary>
        /// 根据相机序列号实例化相机
        /// </summary>
        /// <param name="SN"></param>
        //public BaslerCameraClass(string SN)
        //{
        //    baslerCamera = new Camera(SN);
        //}

        /// <summary>
        /// 根据相机UserID实例化相机
        /// </summary>
        /// <param name="UserID"></param>
        public BaslerCameraClass(string UserID)
        {
            try {
                // 枚举相机列表
                List<ICameraInfo> allCameraInfos = CameraFinder.Enumerate();
                foreach (ICameraInfo cameraInfo in allCameraInfos) {
                    if (UserID == cameraInfo[CameraInfoKey.DeviceIpAddress]) {
                        baslerCamera = new Camera(cameraInfo);
                    }
                }
                if (baslerCamera == null) {
                    MessageBox.Show("未识别到UserID为“" + UserID + "”的相机！",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception e) {
                ShowException(e);
            }
        }

        /******************    相机操作     ******************/
        /// <summary>
        /// 打开相机
        /// </summary>
        public bool OpenBaslerCamera()
        {
            try {
                baslerCamera.Open();
                if (baslerCamera.IsOpen) {
                    //camera.Parameters[PLCamera.AcquisitionFrameRateEnable].SetValue(true); // 限制相机帧率使能
                    //camera.Parameters[PLCamera.AcquisitionFrameRateAbs].SetValue(90);      // 设置最大帧率值
                    baslerCamera.Parameters[PLCameraInstance.MaxNumBuffer].SetValue(10);     // 设置内存中接收图像缓冲区大小
                    strModelName = baslerCamera.CameraInfo[CameraInfoKey.ModelName];         // 获取相机型号
                    strSerialNumber = baslerCamera.CameraInfo[CameraInfoKey.SerialNumber];   // 获取相机序列号
                    strUserID = baslerCamera.CameraInfo[CameraInfoKey.UserDefinedName];      // 获取相机用户自定义名称
                    imageWidth = (int)baslerCamera.Parameters[PLCamera.Width].GetValue();    // 获取图像宽 
                    imageHeight = (int)baslerCamera.Parameters[PLCamera.Height].GetValue();  // 获取图像高
                    payloadSize = imageWidth * imageHeight;                                  // 计算图像分辨率
                    GetMinMaxExposureTime();                                                 // 获取最大最小曝光值 
                    GetMinMaxGain();                                                         // 获取最大最小增益值
                    baslerCamera.StreamGrabber.ImageGrabbed += OnImageGrabbed;               // 注册采集回调函数
                    baslerCamera.ConnectionLost += OnConnectionLost;                         // 注册掉线回调函数
                    return true;
                }
                return false;
            }
            catch (Exception e) {
                ShowException(e);
                return false;
            }
        }

        /// <summary>
        /// 关闭相机,释放相关资源
        /// </summary>
        public void CloseBaslerCamera()
        {
            try {
                baslerCamera.Close();
                if (latestFrameAddress != null) {
                    Marshal.FreeHGlobal(latestFrameAddress);
                    latestFrameAddress = IntPtr.Zero;
                }
            }
            catch (Exception e) {
                ShowException(e);
            }
        }

        /// <summary>
        /// 单张采集照片
        /// </summary>
        public bool IsCameraGenerateSinglePicture()
        {
            try {
                if (baslerCamera.StreamGrabber.IsGrabbing) {
                    MessageBox.Show("相机当前正处于采集状态！", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                } else {
                    baslerCamera.Parameters[PLCamera.AcquisitionMode].SetValue("SingleFrame");
                    baslerCamera.StreamGrabber.Start(1, GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);
                    stopWatch.Restart();    // 重启采集时间计时器  
                    return true;
                }
            }
            catch (Exception e) {
                ShowException(e);
                return false;
            }
        }

        /// <summary>
        /// 开始连续采集
        /// </summary>
        public bool IsCameraStartConsecutiveCollection()
        {
            try {
                if (baslerCamera.StreamGrabber.IsGrabbing) {
                    MessageBox.Show("相机当前正处于采集状态！", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                } else {
                    baslerCamera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                    baslerCamera.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);
                    stopWatch.Restart();    // 重启采集时间计时器   
                    return true;
                }
            }
            catch (Exception e) {
                ShowException(e);
                return false;
            }
        }

        /// <summary>
        /// 停止连续采集
        /// </summary>
        public void StopCameraConsecutiveCollection()
        {
            try {
                if (baslerCamera.StreamGrabber.IsGrabbing) {
                    baslerCamera.StreamGrabber.Stop();
                }
            }
            catch (Exception e) {
                ShowException(e);
            }
        }

        /******************    相机参数设置   ********************/
        /// <summary>
        /// 设置Gige相机心跳时间
        /// 对于网络接口的工业相机才需要设置心跳时间，对于USB接口的相机不需要
        /// </summary>
        /// <param name="value"></param>
        public void SetHeartBeatTime(long value)
        {
            try {
                // 判断是否是网口相机，网口相机才有心跳时间
                if (baslerCamera.GetSfncVersion() < Sfnc2_0_0) {

                    baslerCamera.Parameters[PLGigECamera.GevHeartbeatTimeout].SetValue(value);
                }
            }
            catch (Exception e) {
                ShowException(e);
            }
        }

        /// <summary>
        /// 设置相机曝光时间
        /// </summary>
        /// <param name="value"></param>
        public void SetExposureTime(long value)
        {
            try {
                // Some camera models may have auto functions enabled. 
                // To set the ExposureTime value to a specific value,
                // the ExposureAuto function must be disabled first (if ExposureAuto is available).
                // Set ExposureAuto to Off if it is writable.
                baslerCamera.Parameters[PLCamera.ExposureAuto].TrySetValue(PLCamera.ExposureAuto.Off);
                // Set ExposureMode to Timed if it is writable.
                baslerCamera.Parameters[PLCamera.ExposureMode].TrySetValue(PLCamera.ExposureMode.Timed); 

                // 网口相机
                if (baslerCamera.GetSfncVersion() < Sfnc2_0_0) {
                    // In previous SFNC versions, ExposureTimeRaw is an integer parameter,单位us
                    // integer parameter的数据，设置之前，需要进行有效值整合，否则可能会报错
                    long min = baslerCamera.Parameters[PLCamera.ExposureTimeRaw].GetMinimum();
                    long max = baslerCamera.Parameters[PLCamera.ExposureTimeRaw].GetMaximum();
                    long incr = baslerCamera.Parameters[PLCamera.ExposureTimeRaw].GetIncrement();
                    if (value < min) {
                        value = min;
                    } else if (value > max) {
                        value = max;
                    } else {
                        value = min + (((value - min) / incr) * incr);
                    }
                    baslerCamera.Parameters[PLCamera.ExposureTimeRaw].SetValue(value);

                    // Or,here, we let pylon correct the value if needed.
                    //baslerCamera.Parameters[PLCamera.ExposureTimeRaw].SetValue(value, IntegerValueCorrection.Nearest);
                } else {
                    // For SFNC 2.0 cameras, e.g. USB3 Vision cameras
                    // In SFNC 2.0, ExposureTimeRaw is renamed as ExposureTime,is a float parameter, 单位us.
                    baslerCamera.Parameters[PLUsbCamera.ExposureTime].SetValue((double)value);
                }
            }
            catch (Exception e) {
                ShowException(e);
            }
        }

        /// <summary>
        /// 获取最小最大曝光时间
        /// </summary>
        public void GetMinMaxExposureTime()
        {
            try {
                if (baslerCamera.GetSfncVersion() < Sfnc2_0_0) {
                    // 网口相机
                    minExposureTime = baslerCamera.Parameters[PLCamera.ExposureTimeRaw].GetMinimum();
                    maxExposureTime = baslerCamera.Parameters[PLCamera.ExposureTimeRaw].GetMaximum();
                } else {
                    // USB相机
                    minExposureTime = (long)baslerCamera.Parameters[PLUsbCamera.ExposureTime].GetMinimum();
                    maxExposureTime = (long)baslerCamera.Parameters[PLUsbCamera.ExposureTime].GetMaximum();
                }
            }
            catch (Exception e) {
                ShowException(e);
            }
        }

        /// <summary>
        /// 设置增益
        /// </summary>
        /// <param name="value"></param>
        public void SetGain(long value)
        {
            try {
                // Some camera models may have auto functions enabled. To set the gain value to a specific value,
                // the Gain Auto function must be disabled first (if gain auto is available).
                // Set GainAuto to Off if it is writable.
                baslerCamera.Parameters[PLCamera.GainAuto].TrySetValue(PLCamera.GainAuto.Off); 
                if (baslerCamera.GetSfncVersion() < Sfnc2_0_0) {
                    // 网口相机
                    // Some parameters have restrictions. 
                    // You can use GetIncrement/GetMinimum/GetMaximum to make sure you set a valid value.                              
                    // In previous SFNC versions, GainRaw is an integer parameter.
                    // integer parameter的数据，设置之前，需要进行有效值整合，否则可能会报错
                    long min = baslerCamera.Parameters[PLCamera.GainRaw].GetMinimum();
                    long max = baslerCamera.Parameters[PLCamera.GainRaw].GetMaximum();
                    long incr = baslerCamera.Parameters[PLCamera.GainRaw].GetIncrement();
                    if (value < min) {
                        value = min;
                    } else if (value > max) {
                        value = max;
                    } else {
                        value = min + (((value - min) / incr) * incr);
                    }
                    baslerCamera.Parameters[PLCamera.GainRaw].SetValue(value);

                    //// Or,here, we let pylon correct the value if needed.
                    //camera.Parameters[PLCamera.GainRaw].SetValue(value, IntegerValueCorrection.Nearest);
                } else {
                    // For SFNC 2.0 cameras, e.g. USB3 Vision cameras
                    // In SFNC 2.0, Gain is a float parameter.
                    baslerCamera.Parameters[PLUsbCamera.Gain].SetValue(value);
                }
            }
            catch (Exception e) {
                ShowException(e);
            }
        }

        /// <summary>
        /// 获取相机最小最大增益
        /// </summary>
        public void GetMinMaxGain()
        {
            try {
                if (baslerCamera.GetSfncVersion() < Sfnc2_0_0) {
                    minGain = baslerCamera.Parameters[PLCamera.GainRaw].GetMinimum();
                    maxGain = baslerCamera.Parameters[PLCamera.GainRaw].GetMaximum();
                } else {
                    minGain = (long)baslerCamera.Parameters[PLUsbCamera.Gain].GetMinimum();
                    maxGain = (long)baslerCamera.Parameters[PLUsbCamera.Gain].GetMaximum();
                }
            }
            catch (Exception e) {
                ShowException(e);
            }
        }

        /// <summary>
        /// 设置相机Freerun模式
        /// </summary>
        public void SetFreerun()
        {
            try {
                // Set an enum parameter.
                if (baslerCamera.GetSfncVersion() < Sfnc2_0_0) {
                    if (baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.AcquisitionStart))
                    {
                        if (baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameStart)) {
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.AcquisitionStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.Off);
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.Off);
                        } else {
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.AcquisitionStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.Off);
                        }
                    }
                } else {
                    // For SFNC 2.0 cameras, e.g. USB3 Vision cameras
                    if (baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameBurstStart)) {
                        if (baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameStart)) {
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameBurstStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.Off);
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.Off);
                        } else {
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameBurstStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.Off);
                        }
                    }
                }
                stopWatch.Restart();    // ****  重启采集时间计时器   ****
            }
            catch (Exception e) {
                ShowException(e);
            }
        }

        /// <summary>
        /// 设置相机软触发模式
        /// </summary>
        public void SetSoftwareTrigger()
        {
            try {
                // Set an enum parameter.
                if (baslerCamera.GetSfncVersion() < Sfnc2_0_0) {
                    if (baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.AcquisitionStart)) {
                        if (baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameStart)) {
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.AcquisitionStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.Off);
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.On);
                            baslerCamera.Parameters[PLCamera.TriggerSource].TrySetValue(PLCamera.TriggerSource.Software);
                        } else {
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.AcquisitionStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.On);
                            baslerCamera.Parameters[PLCamera.TriggerSource].TrySetValue(PLCamera.TriggerSource.Software);
                        }
                    }
                }  else {
                    // USB3 Vision cameras
                    if (baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameBurstStart)) {
                        if (baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameStart)) {
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameBurstStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.Off);
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.On);
                            baslerCamera.Parameters[PLCamera.TriggerSource].TrySetValue(PLCamera.TriggerSource.Software);
                        } else {
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameBurstStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.On);
                            baslerCamera.Parameters[PLCamera.TriggerSource].TrySetValue(PLCamera.TriggerSource.Software);
                        }
                    }
                }
                stopWatch.Reset();    // ****  重置采集时间计时器   ****
            }
            catch (Exception e) {
                ShowException(e);
            }
        }

        /// <summary>
        /// 发送软触发命令
        /// </summary>
        public void SendSoftwareExecute()
        {
            try {
            try {
                if (baslerCamera.WaitForFrameTriggerReady(1000, TimeoutHandling.ThrowException)) {
                    baslerCamera.ExecuteSoftwareTrigger();
                    stopWatch.Restart();    // ****  重启采集时间计时器   ****
                }
            }
            catch (Exception exception) {
                ShowException(exception);
            }
        }

        /// <summary>
        /// 设置相机外触发模式
        /// </summary>
        public void SetExternTrigger()
        {
            try {
                if (baslerCamera.GetSfncVersion() < Sfnc2_0_0) {
                    if (baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.AcquisitionStart)) {
                        if (baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameStart)) {
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.AcquisitionStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.Off);
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.On);
                            baslerCamera.Parameters[PLCamera.TriggerSource].TrySetValue(PLCamera.TriggerSource.Line1);
                        } else {
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.AcquisitionStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.On);
                            baslerCamera.Parameters[PLCamera.TriggerSource].TrySetValue(PLCamera.TriggerSource.Line1);
                        }
                    }
                    //Sets the trigger delay time in microseconds.
                    baslerCamera.Parameters[PLCamera.TriggerDelayAbs].SetValue(0);        // 设置触发延时
                    //Sets the absolute value of the selected line debouncer time in microseconds
                    baslerCamera.Parameters[PLCamera.LineSelector].TrySetValue(PLCamera.LineSelector.Line1);
                    baslerCamera.Parameters[PLCamera.LineMode].TrySetValue(PLCamera.LineMode.Input);
                    baslerCamera.Parameters[PLCamera.LineDebouncerTimeAbs].SetValue(0);       // 设置去抖延时，过滤触发信号干扰

                } else {
                    // USB3 Vision cameras
                    if (baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameBurstStart)) {
                        if (baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameStart)) {
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameBurstStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.Off);
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.On);
                            baslerCamera.Parameters[PLCamera.TriggerSource].TrySetValue(PLCamera.TriggerSource.Line1);
                        } else {
                            baslerCamera.Parameters[PLCamera.TriggerSelector].TrySetValue(PLCamera.TriggerSelector.FrameBurstStart);
                            baslerCamera.Parameters[PLCamera.TriggerMode].TrySetValue(PLCamera.TriggerMode.On);
                            baslerCamera.Parameters[PLCamera.TriggerSource].TrySetValue(PLCamera.TriggerSource.Line1);
                        }
                    }

                    //Sets the trigger delay time in microseconds.//float
                    baslerCamera.Parameters[PLCamera.TriggerDelay].SetValue(0);       // 设置触发延时
                    //Sets the absolute value of the selected line debouncer time in microseconds
                    baslerCamera.Parameters[PLCamera.LineSelector].TrySetValue(PLCamera.LineSelector.Line1);
                    baslerCamera.Parameters[PLCamera.LineMode].TrySetValue(PLCamera.LineMode.Input);
                    baslerCamera.Parameters[PLCamera.LineDebouncerTime].SetValue(0);       // 设置去抖延时，过滤触发信号干扰

                }
                stopWatch.Reset();    // ****  重置采集时间计时器   ****
            }
            catch (Exception e) {
                ShowException(e);
            }
        }

        /// <summary>
        /// 保存图像
        /// </summary>
        /// <param name="path">保存图像的路径</param>
        /// <param name="address">图像地址</param>
        /// <param name="width">图像宽</param>
        /// <param name="height">图像高</param>
        public void SaveImage(string path, IntPtr address, int width, int height, bool isColor)
        {
            if (isColor == false) {
                ImagePersistence.Save(ImageFileFormat.Bmp, path, address, width * height, 
                    PixelType.Mono8, width, height, 0, ImageOrientation.TopDown);
            } else {
                ImagePersistence.Save(ImageFileFormat.Bmp, path, address, width * height * 3, 
                    PixelType.BGR8packed, width, height, 0, ImageOrientation.TopDown);
            }

        }


        /****************  图像响应事件回调函数  ****************/
        // 相机取像回调函数.
        private void OnImageGrabbed(Object sender, ImageGrabbedEventArgs e)
        {
            try {
                // Acquire the image from the camera. 
                // Only show the latest image. The camera may acquire images faster than the images can be displayed.
                // Get the grab result.
                IGrabResult grabResult = e.GrabResult;
                // Check if the image can be displayed.
                if (grabResult.GrabSucceeded) {
                    grabTime = stopWatch.ElapsedMilliseconds;
                    // 抛出计算采集时间处理事件
                    eventComputeGrabTime(grabTime);
                    // 判断是否是黑白图片格式
                    if (grabResult.PixelTypeValue == PixelType.Mono8) {
                        if (latestFrameAddress == IntPtr.Zero) {
                            latestFrameAddress = Marshal.AllocHGlobal((Int32)(payloadSize));
                        }
                        converter.OutputPixelFormat = PixelType.Mono8;
                        converter.Convert(latestFrameAddress, payloadSize, grabResult);
                        isColor = false;
                    }
                    eventProcessImage(isColor, imageWidth, imageHeight, latestFrameAddress);
                    // pylon 自带窗体显示图像
                    //ImageWindow.DisplayImage(numWindowIndex, grabResult);
                } else {
                    MessageBox.Show("Grab faild!\n" + grabResult.ErrorDescription, 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception exception) {
                ShowException(exception);
            }
            finally {
                // Dispose the grab result if needed for returning it to the grab loop.
                e.DisposeGrabResultIfClone();
            }
        }

        /// <summary>
        /// 掉线重连回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionLost(Object sender, EventArgs e)
        {
            try {
                const int cTimeOutMs = 20;
                System.Threading.Thread.Sleep(100);
                baslerCamera.Close();
                for (int i = 0; i < 1000; i++) {
                    try {
                        baslerCamera.Open(cTimeOutMs, TimeoutHandling.ThrowException);
                        if (baslerCamera.IsOpen) {
                            MessageBox.Show("已重新连接上UserID为 ：" + strUserID + " 的相机！",
                                "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        }
                        //Thread.Sleep(200);
                    }
                    catch {
                        MessageBox.Show("请重新连接UserID为 ：" + strUserID + " 的相机！",
                            "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                if (baslerCamera == null) {
                    MessageBox.Show("重连超时20s:未识别到UserID为 ：" + strUserID + " 的相机！",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                SetHeartBeatTime(5000);
                //baslerCamera.Parameters[PLCamera.AcquisitionFrameRateEnable].SetValue(true);  // 限制相机帧率
                //baslerCamera.Parameters[PLCamera.AcquisitionFrameRateAbs].SetValue(90);
                //baslerCamera.Parameters[PLCameraInstance.MaxNumBuffer].SetValue(10);          // 设置内存中接收图像缓冲区大小
                imageWidth = (int)baslerCamera.Parameters[PLCamera.Width].GetValue();               // 获取图像宽 
                imageHeight = (int)baslerCamera.Parameters[PLCamera.Height].GetValue();              // 获取图像高
                GetMinMaxExposureTime();
                GetMinMaxGain();
                //baslerCamera.StreamGrabber.Start();

            }
            catch (Exception exception) {
                ShowException(exception);
            }
        }



    }
}






