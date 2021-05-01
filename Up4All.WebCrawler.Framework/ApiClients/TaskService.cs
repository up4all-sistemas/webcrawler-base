using iTextSharp.text;
using iTextSharp.text.pdf;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using OpenQA.Selenium;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Up4All.WebCrawler.Framework.Contracts;
using Up4All.WebCrawler.Framework.Entities;
using Up4All.WebCrawler.Framework.Entities.Enums;
using Up4All.WebCrawler.Framework.Extensions.Selenium;

using Image = System.Drawing.Image;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Rectangle = System.Drawing.Rectangle;

namespace Up4All.WebCrawler.Framework.ApiClients
{
    public class TaskService : ITaskService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TaskService> _logger;
        private readonly IChromeService _chromeService;
        private readonly IImageService _imageService;        

        public TaskService(IConfiguration configuration, ILogger<TaskService> logger, IChromeService chromeService, IImageService imageService)
        {
            _configuration = configuration;
            _logger = logger;
            _chromeService = chromeService;            
            _imageService = imageService;
        }

        public Stream CreateEvidenceAsync(Context context)
        {            
            try
            {
                _logger.LogInformation("Starting evidence snapshot");

                var text = GetHeaderText(context);
                var picture = _imageService.CreateFingerPrint(text, GetEntireScreenshot());

                using (var stream = new MemoryStream())
                {
                    picture.Save(stream, ImageFormat.Png);
                    stream.Position = 0;
                    return stream;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on save picture");
                return null;
            }
        }

        public Stream CreateEvidenceAsyncNoFP(Context context)
        {
            try
            {
                _logger.LogInformation("Starting evidence snapshot");
                                
                var pic = _chromeService.Driver.TakeScreenshot();
                _logger.Log(LogLevel.Trace, "Picture saved");

                return pic;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on save picture");
                return null;
            }
        }

        public Stream CreateEvidenceEntireScreen(Context context, int delayTime = 2000)
        {
            try
            {
                if (!OperatingSystem.IsLinux())
                    return null;

                var uid = Guid.NewGuid().ToString();

                var imagename = $"/temp/{uid}.png";

                var exec = _configuration.GetValue(typeof(string), "EntireScreenExec", "import").ToString();
                var args = _configuration.GetValue(typeof(string), "EntireScreenArgs", "-silent -window root -screen").ToString();

                //exec cmd
                Process.Start(exec, $"{args} {imagename}").WaitForExit();
                Thread.Sleep(delayTime);

                var c = 0;
                while (!File.Exists($"{imagename}") && c++ < 50)
                    Thread.Sleep(100);

                if (!File.Exists($"{imagename}"))
                    return null;

                var stream = new MemoryStream();
                var text = GetHeaderText(context);
                using var img = Image.FromFile(imagename);
                using var img2 = _imageService.CreateFingerPrint(text, img);
                img2.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                File.Delete($"{imagename}");

                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on save picture");

                return null;
            }

            
        }
          
        private Image GetEntireScreenshot()
        {
            try
            {
                // Get the total size of the page
                var totalWidth = (int)(long)((IJavaScriptExecutor)_chromeService.Driver).ExecuteScript("return document.body.offsetWidth"); //documentElement.scrollWidth");
                var totalHeight = (int)(long)((IJavaScriptExecutor)_chromeService.Driver).ExecuteScript("return  document.body.parentNode.scrollHeight");
                // Get the size of the viewport
                var viewportWidth = (int)(long)((IJavaScriptExecutor)_chromeService.Driver).ExecuteScript("return document.body.clientWidth"); //documentElement.scrollWidth");
                var viewportHeight = (int)(long)((IJavaScriptExecutor)_chromeService.Driver).ExecuteScript("return window.innerHeight"); //documentElement.scrollWidth");

                // We only care about taking multiple images together if it doesn't already fit
                if (totalWidth <= viewportWidth && totalHeight <= viewportHeight)
                {
                    var screenshot = _chromeService.Driver.TakeScreenshot();
                    return _imageService.ScreenshotToImage(screenshot);
                }
                // Split the screen in multiple Rectangles
                var rectangles = new List<Rectangle>();
                // Loop until the totalHeight is reached
                for (var y = 0; y < totalHeight; y += viewportHeight)
                {
                    var newHeight = viewportHeight;
                    // Fix if the height of the element is too big
                    if (y + viewportHeight > totalHeight)
                    {
                        newHeight = totalHeight - y;
                    }
                    // Loop until the totalWidth is reached
                    for (var x = 0; x < totalWidth; x += viewportWidth)
                    {
                        var newWidth = viewportWidth;
                        // Fix if the Width of the Element is too big
                        if (x + viewportWidth > totalWidth)
                        {
                            newWidth = totalWidth - x;
                        }
                        // Create and add the Rectangle
                        var currRect = new Rectangle(x, y, newWidth, newHeight);
                        rectangles.Add(currRect);
                    }
                }
                // Build the Image
                var stitchedImage = new Bitmap(totalWidth, totalHeight);
                // Get all Screenshots and stitch them together
                var previous = Rectangle.Empty;
                foreach (var rectangle in rectangles)
                {
                    // Calculate the scrolling (if needed)
                    if (previous != Rectangle.Empty)
                    {
                        var xDiff = rectangle.Right - previous.Right;
                        var yDiff = rectangle.Bottom - previous.Bottom;
                        // Scroll
                        ((IJavaScriptExecutor)_chromeService.Driver).ExecuteScript(String.Format("window.scrollBy({0}, {1})", xDiff, yDiff));
                    }
                    // Take Screenshot
                    var screenshot = _chromeService.Driver.TakeScreenshot();
                    // Build an Image out of the Screenshot
                    var screenshotImage = _imageService.ScreenshotToImage(screenshot);
                    // Calculate the source Rectangle
                    var sourceRectangle = new Rectangle(viewportWidth - rectangle.Width, viewportHeight - rectangle.Height, rectangle.Width, rectangle.Height);
                    // Copy the Image
                    using (var graphics = Graphics.FromImage(stitchedImage))
                    {
                        graphics.DrawImage(screenshotImage, rectangle, sourceRectangle, GraphicsUnit.Pixel);
                    }
                    // Set the Previous Rectangle
                    previous = rectangle;
                }
                return stitchedImage;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on create image");

                var picture = _chromeService.Driver.TakeScreenshot();
                return _imageService.ScreenshotToImage(picture);
            }
        }

        public string CleanAccents(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public Stream GeneratePdf(Image img)
        {
            var pdf = new Document(PageSize.A4);

            var str = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdf, str);
            pdf.Open();

            var pimg = iTextSharp.text.Image.GetInstance(img, BaseColor.White);
            var prop = pimg.PlainHeight / pimg.PlainWidth;
            pimg.ScaleToFit(PageSize.A4.Width, PageSize.A4.Height * prop);
            pimg.SetAbsolutePosition(0, PageSize.A4.Height - pimg.ScaledHeight);
            writer.DirectContent.AddImage(pimg);
            pdf.Close();

            return str;
        }

        public bool CompareName(string sourceName, string contextName)
        {
            sourceName = CleanAccents(sourceName.ToUpper());
            contextName = CleanAccents(contextName.ToUpper());

            var splnome = sourceName.ToUpper().Split(' ');
            var splFullname = contextName.Split(' ');

            var intA = splnome.Except(splFullname);
            var intB = splFullname.Except(splnome);

            return !intA.Any() && !intB.Any();
        }

        private string GetHeaderText(Context context)
        {
            return $"Consulta em {DateTime.Now:dd/MM/yyyy HH:mm} | BotName: {context.BotName} | Task: {context.Task.TaskName}";
        }

        public Task SaveAsync(Context context)
        {
            try
            {
                if (context.Result.Status == TaskResultEnum.None)
                    context.Result.SetAsFailed();

                _logger.LogInformation($"Task {context.Task.TaskId} ended with status {context.Result.Status.Name}");                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on save task", context.Result.Status.Name);
                throw ex;
            }
        }
    }
}