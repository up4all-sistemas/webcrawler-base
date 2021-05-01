using _2CaptchaAPI;
using _2CaptchaAPI.Enums;

using Microsoft.Extensions.Logging;

using Polly;

using System;
using System.IO;
using System.Threading.Tasks;

using Up4All.WebCrawler.Framework.Contracts;
using Up4All.WebCrawler.Framework.Entities;
using Up4All.WebCrawler.Framework.Handlers.Exception;

namespace Up4All.WebCrawler.Framework.CaptchaSolvers
{
    public class TwoCaptchaSolver : ICaptchaSolver
    {
        private readonly ILogger _log;
        private readonly CaptchaConfiguration _capConfig;
        private readonly _2Captcha _client;

        public TwoCaptchaSolver(CaptchaConfiguration capConfig, ILogger<ICaptchaSolver> logService)
        {
            _capConfig = capConfig;
            _log = logService;
            _client = GetClient();
        }

        private _2Captcha GetClient()
        {
            var cli = new _2Captcha(_capConfig.TwoCaptchaApiKey);
            return cli;
        }

        private async Task CheckBalance()
        {
            var saldo = await _client.GetBalance();

            if (saldo.Success)
            {
                double.TryParse(saldo.Response, out var sld);

                if(sld < 10)
                    throw new CaptchaNotSolvedException("Balance less than US$1");
            }
        }

        public void ResolveCaptcha(Stream image, Func<string, bool> callback)
        {
            CheckBalance().Wait();

            var bff = new byte[image.Length];
            image.Read(bff, 0, bff.Length);
            var b64image = Convert.ToBase64String(bff);

            var result = _client.SolveImage(b64image, FileType.Png).GetAwaiter().GetResult();

            if(!result.Success)
                throw new CaptchaNotSolvedException("Captcha not solved");

            callback(result.Response);
        }

        public void ResolveCaptcha(Func<Stream> captureImage, Func<string, bool> callback)
        {
            CheckBalance().Wait();

            var result = Policy
                .Handle<Exception>()
                .WaitAndRetry(_capConfig.RetryBy, (i) => TimeSpan.FromMilliseconds(i * 500))
                .ExecuteAndCapture(() =>
                {
                    var img = captureImage();

                    var bff = new byte[img.Length];
                    img.Read(bff, 0, bff.Length);
                    var b64image = Convert.ToBase64String(bff);

                    var res = _client.SolveImage(b64image, FileType.Png).GetAwaiter().GetResult();

                    if(!res.Success)
                        throw new CaptchaNotSolvedException("Captcha not solved");

                    callback(res.Response);
                });

            if (result.Outcome != OutcomeType.Successful)
                throw result.FinalException;
        }

        public void ResolveReCaptcha(string siteUrl, string siteKey, Func<string, bool> onSucces, Action<string> onFaliure)
        {
            CheckBalance().Wait();

            var result = _client.SolveReCaptchaV2(siteKey, siteUrl).GetAwaiter().GetResult();

            if (!result.Success)
                throw new CaptchaNotSolvedException("Captcha not solved");

            onSucces(result.Response);
        }

        public void ResolveReCaptchaV3(string siteUrl, string siteKey, string action, Func<string, bool> onSucces, Action<string> onFaliure)
        {
            var result = _client.SolveReCaptchaV3(siteKey, siteUrl, action).GetAwaiter().GetResult();

            if (!result.Success)
                throw new CaptchaNotSolvedException("Captcha not solved");

            onSucces(result.Response);
        }

        public void ResolveHCaptcha(string siteUrl, string siteKey, Func<string, bool> onSucces, Action<string> onFaliure)
        {
            CheckBalance().Wait();

            var res = Policy
                .Handle<Exception>()
                .WaitAndRetry(_capConfig.RecaptchaRetryBy, (i) => TimeSpan.FromSeconds(_capConfig.RecaptchSecondsBetweenRetry))
                .ExecuteAndCapture(() =>
                {
                    try
                    {
                        var result = _client.SolveHCaptcha(siteKey, siteUrl).GetAwaiter().GetResult();

                        if (string.IsNullOrEmpty(result.Response) || !onSucces(result.Response))
                            throw new CaptchaNotSolvedException("Captcha not solved");
                    }
                    catch (Exception ex)
                    {
                        onFaliure(ex.Message);
                        throw ex;
                    }
                });

            if (res.Outcome == OutcomeType.Failure)
                throw new CaptchaNotSolvedException("Captcha not solved");
        }
    }
}
