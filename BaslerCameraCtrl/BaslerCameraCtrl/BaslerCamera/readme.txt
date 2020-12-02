// 1 工业相机开发探索

1、预定义相机参数设置
配置类提供了一组预定义的相机参数配置，这个类可以被添加到CameraOpened事件中。
如果相机被打开，配置方法将自动调用，相机配置也相应的改变。也可以创建自己的配置方法，并将它们添加到CameraOpened事件中。
这样的方式使相机打开的时候参数总是正确的。例如在设置采集方式时的配置：
camera.CameraOpened += Configuration.AcquireContinuous;
camera.Open();




