using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

using Polly;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Up4All.WebCrawler.Framework.ChromePlugins;
using Up4All.WebCrawler.Framework.Contracts;
using Up4All.WebCrawler.Framework.Extensions.Selenium;
using Up4All.WebCrawler.Framework.Handlers.Exception;
using Up4All.WebCrawler.Framework.Options;

namespace Up4All.WebCrawler.Framework.Services
{
    public class ChromeService : IChromeService, IDisposable
    {   
        private readonly ILogger<ChromeService> _logService;
        private readonly CrawlerChromeOptions _options;

        public IWebDriver Driver { get; set; }

        private readonly string _chromepath;
        private readonly string _driverpath;
        private readonly string _downloadPath;

        public ChromeService(ILogger<ChromeService> logService, IOptions<CrawlerOptions> opts)
        {
            _logService = logService;
            _options = opts.Value.Chrome;            

            _chromepath = string.IsNullOrEmpty(_options.ChromeBinaryPath) ? "/opt/google/chrome/chrome" : _options.ChromeBinaryPath;

            var defaultChromedriverPath = OperatingSystem.IsLinux() ? "/opt/selenium/" : AppDomain.CurrentDomain.BaseDirectory;
            _driverpath = string.IsNullOrEmpty(_options.ChromeDriverPath) ? defaultChromedriverPath : _options.ChromeDriverPath;
            _downloadPath = string.IsNullOrEmpty(_options.DownloadPath) ? AppContext.BaseDirectory : _options.DownloadPath;

        }

        public void ConfigureWebBrowser()
        {
            try
            {
                _logService.LogInformation("Starting browser configuration");

                var driver = ChromeDriverService.CreateDefaultService(_driverpath);
                driver.HideCommandPromptWindow = true;

                var options = new ChromeOptions
                {
                    AcceptInsecureCertificates = true,
                    LeaveBrowserRunning = false,
                    PageLoadStrategy = PageLoadStrategy.Default,
                    //UnhandledPromptBehavior = UnhandledPromptBehavior.Ignore,                                        
                };

                if (_options.Proxy.Enabled && !_options.Proxy.Authenticated)
                {
                    var ip = _options.Proxy.Ips[new Random().Next(0, _options.Proxy.Ips.Count() - 1)];
                    options.Proxy = new Proxy
                    {
                        FtpProxy = ip,
                        HttpProxy = ip,
                        SslProxy = ip
                    };
                }

                options.AddArgument("window-size=1440,900");
                options.AddArgument("disable-default-apps");
                options.AddArgument("disable-infobars");
                options.AddArgument("ignore-certificate-errors");
                options.AddArgument("ignore-ssl-errors");
                options.AddArgument("ignore-certificate-errors-spki-list");
                options.AddArgument("disable-sync");
                options.AddArgument("disable-background-networking");
                options.AddArgument("no-sandbox");                
                options.AddArgument("disable-browser-side-navigation");
                options.AddArgument("disable-gpu");                
                options.AddArgument("--disable-features=RendererCodeIntegrity");
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddExcludedArgument("enable-automation");

                options.AddUserProfilePreference("download.default_directory", _downloadPath);
                options.AddUserProfilePreference("download.prompt_for_download", false);
                options.AddUserProfilePreference("download.directory_upgrade", true);
                options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                

                if (_options.Proxy.Enabled && _options.Proxy.Authenticated)
                {
                    var path = ProxyAuth.GeneratePlugin(_options.Proxy);
                    _logService.LogInformation($"Using Proxy Auth Plugin at {path}");
                    options.AddExtension(path);
                }
                else
                    options.AddArgument("disable-extensions");

                if (_options.HeadlessMode)
                    options.AddArgument("headless");
                else
                {
                    options.AddArgument("--disable-dev-shm-usage");
                    options.AddArgument("--disable-features=VizDisplayCompositor");
                }

                if (OperatingSystem.IsLinux())
                    options.BinaryLocation = _chromepath;

                Driver = new ChromeDriver(driver, options, TimeSpan.FromSeconds(90));

                Driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");

                var pageLoadTimeout = _options.PageLoadTimeOut ?? 30;
                if (pageLoadTimeout > 60)
                    pageLoadTimeout = 60;

                Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(pageLoadTimeout);
                Driver.Manage().Window.Maximize();

            }
            catch (Exception ex)
            {
                _logService.LogError(ex, "Error on setup the browser");
                Driver?.Close();
                throw ex;
            }
        }

        public void ResetBrowser()
        {            
            Driver?.Quit();
            ConfigureWebBrowser();
        }

        public void Dispose()
        {                        
            Driver?.Dispose();
        }

        public IWebElement FindElement(By by, int timeoutInSeconds = 5)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(by));
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new WebCrawlerException<WebDriverTimeoutException>(ex);
            }
        }

        public bool IsAnyTextInPage(params string[] texts)
        {
            return texts.Any(c => Driver.PageSource.Contains(c));
        }

        public bool IsAnyTextInPage(int timeoutInSeconds, params string[] texts)
        {
            if (timeoutInSeconds > 0)
            {
                try
                {
                    var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutInSeconds));
                    return wait.Until(drv => texts.Any(c => drv.PageSource.Contains(c)));
                }
                catch (WebDriverTimeoutException)
                {
                }
            }

            return IsAnyTextInPage(texts);
        }

        public bool IsTextInPage(string text, int timeoutInSeconds = 5)
        {
            if (timeoutInSeconds > 0)
            {
                try
                {
                    var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutInSeconds));
                    return wait.Until(drv => drv.PageSource.Contains(text));
                }
                catch (WebDriverTimeoutException)
                {
                }
            }

            return Driver.PageSource.Contains(text);
        }

        public ReadOnlyCollection<IWebElement> FindElements(By by, int timeoutInSeconds = 5)
        {
            return Driver.FindElements(by);
        }

        public bool PageCrashed()
        {
            var pagetext = Driver.PageSource;

            return !Driver.FindElements(By.TagName("body")).Any()
                || pagetext.Contains("Esta página não está funcionando")
                || pagetext.Contains("Não é possível acessar esse site");
        }

        public void TryUpdatePage()
        {
            var result = Policy
                .Handle<Exception>()
                .WaitAndRetry(2, i => TimeSpan.FromMilliseconds(i * 1000))
                .ExecuteAndCapture(() =>
                {
                    Driver.Navigate().Refresh();

                    if (PageCrashed())
                        throw new UnavailableSourceException();
                });

            if (result.Outcome != OutcomeType.Successful)
                throw result.FinalException;
        }

        public void NavigateTo(string url)
        {
            var result = Policy
                .Handle<Exception>()
                .WaitAndRetry(1, (i) => TimeSpan.FromMilliseconds(i * 1000), (ex, ts, i, ctx) =>
                {
                    _logService.LogError(ex, $"Tentativa de navegação {i}");
                })
                .ExecuteAndCapture(() =>
                {
                    Driver.Navigate().GoToUrl(url);

                    if (PageCrashed())
                        throw new UnavailableSourceException();
                });

            if (result.Outcome == OutcomeType.Failure)
                throw result.FinalException;
        }

        public void SwitchToNewTab()
        {
            var windows = Driver.WindowHandles;
            Driver.SwitchTo().Window(windows.Last());
        }

        public void CloseNewTab()
        {
            var windows = Driver.WindowHandles;
            Driver.Close();
            Driver.SwitchTo().Window(windows.First());
        }
    }
}