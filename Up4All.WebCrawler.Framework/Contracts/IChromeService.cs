using OpenQA.Selenium;
using System.Collections.ObjectModel;

namespace Up4All.WebCrawler.Framework.Contracts
{
    public interface IChromeService
    {
        IWebDriver Driver { get; set; }

        void ConfigureWebBrowser();

        void ResetBrowser();

        void NavigateTo(string url);

        IWebElement FindElement(By by, int timeoutInSeconds = 1);

        bool IsAnyTextInPage(params string[] texts);

        bool IsAnyTextInPage(int timeoutInSeconds, params string[] texts);

        bool IsTextInPage(string text, int timeoutInSeconds = 1);

        bool PageCrashed();

        void TryUpdatePage();

        ReadOnlyCollection<IWebElement> FindElements(By by, int timeoutInSeconds = 1);
    }
}