using System.Drawing;
using System.IO;

namespace Up4All.WebCrawler.Framework.Contracts
{
    public interface IImageService
    {
        Bitmap CropImage(Image originalImage, Rectangle sourceRectangle,
                         Rectangle? destinationRectangle = null);

        Image ScreenshotToImage(Stream screenshot);

        Image CreateFingerPrint(string text, Image screenshot);
    }
}
