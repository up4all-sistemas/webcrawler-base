using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using WDSE;
using WDSE.Decorators;
using WDSE.ScreenshotMaker;

namespace Up4All.WebCrawler.Framework.Extensions.Selenium
{
    public static class WebDriverExtension
    {
        public static void WaitUntilDocumentIsReady(this IWebDriver driver, int timeoutInSeconds = 20)
        {
            var javaScriptExecutor = driver as IJavaScriptExecutor;
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));

            try
            {
                Func<IWebDriver, bool> readyCondition = webDriver => (bool)javaScriptExecutor.ExecuteScript("return (document.readyState == 'complete' && jQuery.active == 0)");
                wait.Until(readyCondition);
            }
            catch (InvalidOperationException)
            {
                wait.Until(wd => javaScriptExecutor.ExecuteScript("return document.readyState").ToString() == "complete");
            }
        }

        public static void WaitForAjaxRequests(this IWebDriver driver, int timeout = int.MaxValue)
        {
            var jsdriver = (IJavaScriptExecutor)driver;

            object conns;

            do
            {
                conns = jsdriver.ExecuteScript("return window.openHTTPs");

                if (conns is long)
                {                    
                    Thread.Sleep(1000);
                    timeout--;
                }
                else
                {
                    InjectAjaxEventWatcher(driver);
                    conns = 1L;
                }

                if (timeout <= 0)
                    break;
            }
            while ((long)conns > 0);
        }

        public static bool IsDialogPresent(this IWebDriver driver, int timeoutInSecs = 10)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSecs));
                var alert = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent());
                return (alert != null);
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public static Stream TakeScreenshotFromElement(this IWebDriver driver, By elem)
        {
            var sm = new ScreenshotMaker();
            var oelem = new OnlyElementDecorator(sm);
            oelem.SetElement(elem);
            var ss = driver.TakeScreenshot(oelem);
            return new MemoryStream(ss);
        }

        public static Stream TakeScreenshotFromElement(this IWebDriver driver, IWebElement elem)
        {
            var ss = ((ITakesScreenshot)driver).GetScreenshot();

            using (var mss = new MemoryStream(ss.AsByteArray))
            {
                using (var screen = Image.FromStream(mss) as Bitmap)
                {
                    var x = elem.Location.X;
                    var y = elem.Location.Y;
                    var w = elem.Size.Width;
                    var h = elem.Size.Height;

                    var img = screen.Clone(new Rectangle(x, y, w, h), screen.PixelFormat);

                    var ms = new MemoryStream();
                    img.Save(ms, ImageFormat.Png);
                    return ms;
                }
            }
        }

        public static Stream TakeScreenshot(this IWebDriver driver, string screenshotsPasta = "")
        {
            var camera = driver as ITakesScreenshot;
            var foto = camera.GetScreenshot();

            if (!string.IsNullOrWhiteSpace(screenshotsPasta))
                foto.SaveAsFile(screenshotsPasta, ScreenshotImageFormat.Png);

            Stream stream = new MemoryStream(foto.AsByteArray);
            return stream;
        }

        public static void ScrollToElement(this IWebDriver driver, By by)
        {
            var element = driver.FindElement(by);
            var act = new Actions(driver);
            act.MoveToElement(element);
            act.Perform();
        }

        public static object ExecuteScript(this IWebDriver webDriver, string jsCommand)
        {
            var javaScriptExecutor = webDriver as IJavaScriptExecutor;

            return javaScriptExecutor.ExecuteScript(jsCommand);
        }

        private static void InjectAjaxEventWatcher(IWebDriver driver)
        {
            var jsdriver = driver as IJavaScriptExecutor;
            var conns = jsdriver.ExecuteScript("return window.openHTTPs");

            if (conns is long)
                return;

            var script = "  (function() {" +
                "var oldOpen = XMLHttpRequest.prototype.open;" +
                "window.openHTTPs = 0;" +
                "XMLHttpRequest.prototype.open = function(method, url, async, user, pass) {" +
                "window.openHTTPs++;" +
                "this.addEventListener('readystatechange', function() {" +
                "if(this.readyState == 4) {" +
                "window.openHTTPs--;" +
                "}" +
                "}, false);" +
                "oldOpen.call(this, method, url, async, user, pass);" +
                "}" +
                "})();";

            jsdriver.ExecuteScript(script);
        }
    }
}
