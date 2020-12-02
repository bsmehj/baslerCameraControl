/*
 * 说明：此类封装图片处理相关的接口，方便后续工程中直接直接使用
 * 作者：huangjun
 * 时间：2020-12-02
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaslerCameraCtrl.UserMethodClass
{
    class ImageProcessClass
    {
        /// <summary>
        /// 处理图片透明操作
        /// </summary>
        /// <param name="srcImage">原始图片</param>
        /// <param name="opacity">透明度(0.0---1.0)</param>
        /// <returns></returns>
        private Image TransparentImage(Image srcImage, float opacity)
        {
            float[][] nArray ={ new float[] {1, 0, 0, 0, 0},
                                new float[] {0, 1, 0, 0, 0},
                                new float[] {0, 0, 1, 0, 0},
                                new float[] {0, 0, 0, opacity, 0},
                                new float[] {0, 0, 0, 0, 1}};
            ColorMatrix matrix = new ColorMatrix(nArray);
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            Bitmap resultImage = new Bitmap(srcImage.Width, srcImage.Height);
            Graphics g = Graphics.FromImage(resultImage);
            g.DrawImage(srcImage, new Rectangle(0, 0, srcImage.Width, srcImage.Height),
                0, 0, srcImage.Width, srcImage.Height, GraphicsUnit.Pixel, attributes);

            return resultImage;
        }

        /// <summary>
        /// 无损压缩图片
        /// </summary>
        /// <param name="srcImg">原图片地址</param>
        /// <param name="dstImg">压缩后保存图片地址</param>
        /// <param name="dstHeight"></param>
        /// <param name="dstWidth"></param>
        /// <param name="flag">压缩质量（数字越小压缩率越高）1-100</param>
        /// <returns></returns>
        public bool GetPicThumbnail(string srcImg, string dstImg, int dstHeight, int dstWidth, int flag)
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromFile(srcImg);
            ImageFormat tFormat = iSource.RawFormat;
            int sW = 0;
            int sH = 0;
            // 按比例缩放
            Size tem_size = new Size(iSource.Width, iSource.Height);
            if (tem_size.Width > dstHeight || tem_size.Width > dstWidth) {
                if ((tem_size.Width * dstHeight) > (tem_size.Width * dstWidth)) {
                    sW = dstWidth;
                    sH = (dstWidth * tem_size.Height) / tem_size.Width;
                } else {
                    sH = dstHeight;
                    sW = (tem_size.Width * dstHeight) / tem_size.Height;
                }
            } else {
                sW = tem_size.Width;
                sH = tem_size.Height;
            }

            Bitmap ob = new Bitmap(dstWidth, dstHeight);
            Graphics g = Graphics.FromImage(ob);
            g.Clear(Color.WhiteSmoke);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(iSource, new Rectangle((dstWidth - sW) / 2, (dstHeight - sH) / 2, sW, sH), 0, 0, 
                iSource.Width, iSource.Height, GraphicsUnit.Pixel);
            g.Dispose();
            // 以下代码为保存图片时，设置压缩质量  
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag; //设置压缩的比例1-100  
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++) {
                    if (arrayICI[x].FormatDescription.Equals("JPEG")) {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }

                if (jpegICIinfo != null) {
                    ob.Save(dstImg, jpegICIinfo, ep); // dstImg是压缩后的新路径  
                } else {
                    ob.Save(dstImg, tFormat);
                }
                return true;
            } catch {
                return false;
            } finally {
                iSource.Dispose();
                ob.Dispose();
            }
        }


        // <summary>
        /// 无损压缩图片
        /// </summary>
        /// <param name="sFile">原图片地址</param>
        /// <param name="dFile">压缩后保存图片地址</param>
        /// <param name="flag">压缩质量（数字越小压缩率越高）1-100</param>
        /// <param name="size">压缩后图片的最大大小</param>
        /// <param name="sfsc">是否是第一次调用</param>
        /// <returns></returns>
        public bool CompressImage(string sFile, string dFile, int flag = 90, int size = 300, bool sfsc = true)
        {
            // 如果接口是第一次调用，且原始图像的大小, 小于要压缩的目标大小，则直接复制图像结束，返回true
            FileInfo firstFileInfo = new FileInfo(sFile);
            if (sfsc == true && firstFileInfo.Length < size * 1024) {
                firstFileInfo.CopyTo(dFile);
                return true;
            }

            // 压缩过程
            Image iSource = Image.FromFile(sFile);
            ImageFormat tFormat = iSource.RawFormat;
            int dHeight = iSource.Height / 2;
            int dWidth = iSource.Width / 2;
            int sW = 0, sH = 0;
            // 按比例缩放
            Size tem_size = new Size(iSource.Width, iSource.Height);
            if (tem_size.Width > dHeight || tem_size.Width > dWidth) {
                if ((tem_size.Width * dHeight) > (tem_size.Width * dWidth)) {
                    sW = dWidth;
                    sH = (dWidth * tem_size.Height) / tem_size.Width;
                } else {
                    sH = dHeight;
                    sW = (tem_size.Width * dHeight) / tem_size.Height;
                }
            } else {
                sW = tem_size.Width;
                sH = tem_size.Height;
            }

            Bitmap bmp = new Bitmap(dWidth, dHeight);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.WhiteSmoke);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 
                0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);
            g.Dispose();

            // 以下代码为保存图片时，设置压缩质量
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag; // 设置压缩的比例1-100
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++) {
                    if (arrayICI[x].FormatDescription.Equals("JPEG")) {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }

                if (jpegICIinfo != null) {
                    bmp.Save(dFile, jpegICIinfo, ep); // dFile是压缩后的新路径
                    FileInfo fi = new FileInfo(dFile);
                    if (fi.Length > 1024 * size) {
                        flag = flag - 10;
                        CompressImage(sFile, dFile, flag, size, false);
                    }
                }
                else {
                    bmp.Save(dFile, tFormat);
                }
                return true;
            } catch {
                return false;
            } finally
            {
                iSource.Dispose();
                bmp.Dispose();
            }
        }



    }
}
