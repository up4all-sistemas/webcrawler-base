using System;
using System.IO;

namespace Up4All.WebCrawler.Framework.Contracts
{
    public interface ICaptchaSolver
    {
        void ResolveCaptcha(Stream image, Func<string, bool> callback);

        void ResolveCaptcha(Func<Stream> captureImage, Func<string, bool> callback);

        void ResolveReCaptcha(string siteUrl, string siteKey, Func<string, bool> onSucces, Action<string> onFaliure);

        void ResolveReCaptchaV3(string siteUrl, string siteKey, string action, Func<string, bool> onSucces, Action<string> onFaliure);

        void ResolveHCaptcha(string siteUrl, string siteKey, Func<string, bool> onSucces, Action<string> onFaliure);
    }
}
