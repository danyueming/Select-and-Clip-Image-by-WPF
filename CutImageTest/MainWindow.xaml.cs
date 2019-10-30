using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Brush = System.Drawing.Brush;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using System.Drawing.Drawing2D;

namespace CutImageTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadDelegate();

        }

        private void LoadDelegate()
        {
            sourceImg.MouseLeftButtonDown += new MouseButtonEventHandler(Grid_LeftButtonDown);
            sourceImg.MouseRightButtonDown += new MouseButtonEventHandler(Grid_RightButtonDown);
            sourceImg.MouseMove += new MouseEventHandler(Grid_MouseMove);
            sourceImg.MouseUp += new MouseButtonEventHandler(Grid_MouseUp);
        }


        /// <summary>
        /// 起点
        /// </summary>
        private System.Windows.Point startDrag;

        private void Grid_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startDrag = e.GetPosition(sourceImg);

            Canvas.SetZIndex(rectangle, cusCanvas.Children.Count);

            if (!cusCanvas.IsMouseCaptured)  //Capture the mouse
                sourceImg.CaptureMouse();
            sourceImg.Cursor = Cursors.Cross;
        }

        private void Grid_RightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (rectangle.Visibility == Visibility.Visible)
            {
                rectangle.Visibility = Visibility.Hidden;
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (sourceImg.IsMouseCaptured)
            {
                System.Windows.Point currentPoint = e.GetPosition(cusCanvas);

                double x = startDrag.X < currentPoint.X ? startDrag.X : currentPoint.X;
                double y = startDrag.Y < currentPoint.Y ? startDrag.Y : currentPoint.Y;

                if (rectangle.Visibility == Visibility.Hidden)
                    rectangle.Visibility = Visibility.Visible;

                Canvas.SetLeft(rectangle, x);
                Canvas.SetTop(rectangle, y);
                rectangle.Width = Math.Abs(e.GetPosition(cusCanvas).X - startDrag.X); //Set its size,下次点击width为0
                rectangle.Height = Math.Abs(e.GetPosition(cusCanvas).Y - startDrag.Y);
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sourceImg.IsMouseCaptured)
            {
                sourceImg.ReleaseMouseCapture();
                GetCutImage();
            }
            sourceImg.Cursor = Cursors.Arrow;

        }

        private void GetCutImage()
        {
            int tempLeft = (int)Canvas.GetLeft(rectangle);
            int tempTop = (int)Canvas.GetTop(rectangle);
            int tempWidth = (int)rectangle.Width;
            int tempHeight = (int)rectangle.Height;
            System.Drawing.Image currentImage;
            System.Drawing.Image processImage;
            //currentImage = System.Drawing.Image.FromFile(@"D:\公司项目\测试的代码\CutImageTest\CutImageTest\10.jpg");
            currentImage = ImageWork.ImageSourceToBitmap(sourceImg.Source);//获取原图片
            System.Drawing.Rectangle SelectedRect = new Rectangle(tempLeft, tempTop, tempWidth, tempHeight);
            System.Drawing.Rectangle _selectedRect = MapSelectedRegion(sourceImg, SelectedRect, currentImage);
            //System.Drawing.Rectangle _selectedRect = new System.Drawing.Rectangle(tempLeft, tempTop, tempWidth, tempHeight);
            processImage = ImageWork.ClipImage(currentImage,
                _selectedRect,
                (int)targetImg.Width,
               (int)targetImg.Height,
                System.Drawing.Color.Black
                );
            Bitmap bmp = new Bitmap(processImage); // get bitmap
            BitmapImage bmpResource = ImageWork.BitmapToBitmapImage(bmp);
            targetImg.Source = bmpResource;
        }

        /// <summary>
        /// 计算选择区域的缩放比例
        /// </summary>
        /// <param name="MapPbx"></param>
        /// <param name="MapRect"></param>
        /// <param name="SourceImage"></param>
        /// <returns></returns>
        private System.Drawing.Rectangle MapSelectedRegion(FrameworkElement MapPbx, System.Drawing.Rectangle MapRect, System.Drawing.Image SourceImage)
        {

            #region Determine the originally used mapping offsets and dimensions

            int targetMapOffsetX = 0;
            int targetMapOffsetY = 0;
            int targetMapWidth = 0;
            int targetMapHeight = 0;
            float dummyMapRatio = 0f;
            int tempWidth = (int)MapPbx.ActualWidth;
            int tempHeight = (int)MapPbx.ActualHeight;
            ImageWork.FixedSize_ResizeRatios(SourceImage.Width, SourceImage.Height, tempWidth, tempHeight, ref targetMapOffsetX, ref targetMapOffsetY, ref targetMapWidth, ref targetMapHeight, ref dummyMapRatio);

            #endregion

            #region Compute mapped .Left and .Width 

            double dMapPbxWidth = (double)targetMapWidth;
            double xLeft = (double)MapRect.Left;
            double xWidth = (double)MapRect.Width;
            if (targetMapOffsetX > 0)
            {
                double dOffsetX = (double)targetMapOffsetX;
                double dOffsetWidth = (double)targetMapWidth;
                if (xLeft < dOffsetX)
                {
                    xLeft = 0;
                }
                else
                {
                    xLeft -= dOffsetX;
                }
                double xRight = (double)MapRect.Right - dOffsetX;
                if (xRight > dOffsetWidth)
                {
                    xRight = dOffsetWidth;
                }

                xWidth = xRight - xLeft;
                if (xWidth > dMapPbxWidth)
                {
                    xWidth = dMapPbxWidth;
                }
            }
            int _mappedRect_Left = (int)((double)SourceImage.Width * (xLeft / dMapPbxWidth));
            int _mappedRect_Width = (int)((double)SourceImage.Width * (xWidth / dMapPbxWidth));
            #endregion

            #region Compute mapped .Top and .Height 

            double dMapPbxHeight = (double)targetMapHeight;
            double yTop = (double)MapRect.Top;
            double yHeight = (double)MapRect.Height;
            if (targetMapOffsetY > 0)
            {
                double dOffsetY = (double)targetMapOffsetY;
                double dOffsetHeight = (double)targetMapHeight;
                if (yTop < dOffsetY)
                {
                    yTop = 0;
                }
                else
                {
                    yTop -= dOffsetY;
                }
                double yBottom = (double)MapRect.Bottom - dOffsetY;
                if (yBottom > dOffsetHeight)
                {
                    yBottom = dOffsetHeight;
                }

                yHeight = yBottom - yTop;
                if (yHeight > dMapPbxHeight)
                {
                    yHeight = dMapPbxHeight;
                }
            }
            int _mappedRect_Top = (int)((double)SourceImage.Height * (yTop / dMapPbxHeight));
            int _mappedRect_Height = (int)((double)SourceImage.Height * (yHeight / dMapPbxHeight));
            #endregion

            return new System.Drawing.Rectangle(_mappedRect_Left, _mappedRect_Top, _mappedRect_Width, _mappedRect_Height);
        }
    }

    /// <summary>
    ///  图像工具类
    /// </summary>
    public static class ImageWork
    {

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);
        /// <summary>
        /// Bitmap->BitmapSource
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapSource getBitMapSourceFromBitmap(Bitmap bitmap)
        {
            IntPtr intPtrl = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(intPtrl,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(intPtrl);
            return bitmapSource;
        }

        /// <summary>
        ///  Bitmap --> BitmapImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);
                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

        /// <summary>
        /// BitmapSource->Bitmap
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static System.Drawing.Bitmap WpfBitmapSourceToBitmap(BitmapSource s)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(s.PixelWidth, s.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
            System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            s.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }


        /// <summary>
        /// ImageSource --> Bitmap
        /// </summary>
        /// <param name="imageSource"></param>
        /// <returns></returns>
        public static System.Drawing.Bitmap ImageSourceToBitmap(System.Windows.Media.ImageSource imageSource)
        {
            BitmapSource m = (BitmapSource)imageSource;

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(m.PixelWidth, m.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            System.Drawing.Imaging.BitmapData data = bmp.LockBits(
            new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            m.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride); bmp.UnlockBits(data);

            return bmp;
        }


        /// <summary>
        /// 剪切图像
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="sourceRect"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        /// <param name="targetRect"></param>
        /// <param name="bkgColor"></param>
        /// <returns></returns>
        public static System.Drawing.Image ClipImage(System.Drawing.Image sourceImage, System.Drawing.Rectangle sourceRect, int targetWidth, int targetHeight, System.Drawing.Rectangle targetRect, System.Drawing.Color bkgColor)
        {
            System.Drawing.Bitmap returnImage = new System.Drawing.Bitmap(targetWidth, targetHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            //设置目标图像的水平，垂直分辨率
            returnImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

            //创建一个graphics object 
            System.Drawing.Graphics grImage = System.Drawing.Graphics.FromImage(returnImage);

            //清除整个绘图面并以指定的背景色填充
            grImage.Clear(bkgColor);

            //指定在缩放或旋转图像时使用的算法
            grImage.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            grImage.DrawImage(sourceImage,
                targetRect,
                sourceRect,
                System.Drawing.GraphicsUnit.Pixel);

            grImage.Dispose();
            return returnImage;
        }


        /// <summary>
        /// 剪切图片,不需要targetRect参数
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="sourceRect"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        /// <param name="bkgColor"></param>
        /// <returns></returns>
        public static System.Drawing.Image ClipImage(System.Drawing.Image sourceImage, System.Drawing.Rectangle sourceRect, int targetWidth, int targetHeight, System.Drawing.Color bkgColor)
        {
            System.Drawing.Rectangle targetRect = new System.Drawing.Rectangle(0, 0, targetWidth, targetHeight);
            return ClipImage(sourceImage, sourceRect, targetWidth, targetHeight, targetRect, bkgColor);
        }

        /// <summary>
        /// 修正缩放比例
        /// </summary>
        /// <param name="sourceWidth"></param>
        /// <param name="sourceHeight"></param>
        /// <param name="fixedWidth"></param>
        /// <param name="fixedHeight"></param>
        /// <param name="targetMapOffsetX"></param>
        /// <param name="targetMapOffsetY"></param>
        /// <param name="targetMapWidth"></param>
        /// <param name="targetMapHeight"></param>
        /// <param name="targetMapRatio"></param>
        public static void FixedSize_ResizeRatios(
           int sourceWidth, int sourceHeight,
           int fixedWidth, int fixedHeight,
           ref int targetMapOffsetX, ref int targetMapOffsetY,
           ref int targetMapWidth, ref int targetMapHeight,
           ref float targetMapRatio
       )
        {
            targetMapOffsetX = 0;
            targetMapOffsetY = 0;
            targetMapWidth = 0;
            targetMapHeight = 0;
            targetMapRatio = 0f;

            float targetMapRatioWidth = ((float)fixedWidth / (float)sourceWidth);
            float targetMapRatioHeight = ((float)fixedHeight / (float)sourceHeight);
            if (targetMapRatioHeight < targetMapRatioWidth)
            {
                targetMapRatio = targetMapRatioHeight;
                targetMapOffsetX = System.Convert.ToInt16((fixedWidth - (sourceWidth * targetMapRatio)) / 2);
            }
            else
            {
                targetMapRatio = targetMapRatioWidth;
                targetMapOffsetY = System.Convert.ToInt16((fixedHeight - (sourceHeight * targetMapRatio)) / 2);
            }

            targetMapWidth = (int)(sourceWidth * targetMapRatio);
            targetMapHeight = (int)(sourceHeight * targetMapRatio);
        }
    }



    public class GetResource
    {
        public static string GetPictureFilePath(string fileName)
        {
            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string pathPicture = appdataPath + "\\图片\\";
            if (!Directory.Exists(pathPicture))
            {
                Directory.CreateDirectory(pathPicture);
            }
            string filePath = pathPicture + fileName + ".jpg";
            return filePath;
        }
    }


}
