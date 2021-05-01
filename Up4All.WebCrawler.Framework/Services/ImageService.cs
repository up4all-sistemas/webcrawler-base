using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Up4All.WebCrawler.Framework.Contracts;

namespace Up4All.WebCrawler.Framework.Services
{
    public class ImageService : IImageService
    {
        public Bitmap CropImage(Image originalImage, Rectangle sourceRectangle,
                             Rectangle? destinationRectangle = null)
        {
            if (destinationRectangle == null)
                destinationRectangle = new Rectangle(Point.Empty, sourceRectangle.Size);

            var croppedImage = new Bitmap(destinationRectangle.Value.Width, destinationRectangle.Value.Height);

            using (var graphics = Graphics.FromImage(croppedImage))
            {
                graphics.DrawImage(originalImage, destinationRectangle.Value, sourceRectangle, GraphicsUnit.Pixel);
            }

            return croppedImage;
        }

        public Image ScreenshotToImage(Stream screenshot)
        {
            Image screenshotImage;
            using (var memStream = screenshot)
            {
                screenshotImage = Image.FromStream(memStream);
            }
            return screenshotImage;
        }

        public Image CreateFingerPrint(string text, Image screenshot)
        {
            var rh = 30;

            var image = new Bitmap(screenshot.Width, screenshot.Height + rh);
            using (var grap = Graphics.FromImage(image))
            {                
                var bt = new SolidBrush(Color.Black);
                var ft = new Font(SystemFonts.DefaultFont.FontFamily, 20, FontStyle.Regular, GraphicsUnit.Pixel);
                var tsize = grap.MeasureString(text, ft);

                var xm = (int)Math.Floor(image.Width / 2d);
                var tm = (int)Math.Floor(tsize.Width / 2d);

                var rect = new Rectangle(0, 0, image.Width, rh);
                grap.DrawRectangle(new Pen(Color.LightGray), rect);
                grap.FillRectangle(new SolidBrush(Color.LightGray), rect);
                grap.DrawString(text, ft, bt, xm - tm, 3);

                grap.DrawImage(screenshot, 0, rh);
            }

            return image;
        }
    }
}
