using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DeathByCaptcha;
using Up4All.WebCrawler.Framework.Contracts;
using Up4All.WebCrawler.Framework.Entities;
using Up4All.WebCrawler.Framework.Handlers.Exception;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;

namespace Up4All.WebCrawler.Framework.CaptchaSolvers
{
    public class DeadByCaptchaSolver : ICaptchaSolver
    {
        private readonly ILogger<ICaptchaSolver> logService;
        protected readonly CaptchaConfiguration _captchaConfiguration;
        private Client _client;

        public DeadByCaptchaSolver(CaptchaConfiguration captchaConfiguration, ILogger<ICaptchaSolver> logService)
        {
            this.logService = logService;
            _captchaConfiguration = captchaConfiguration;

            _client = new DeathByCaptcha.HttpClient(_captchaConfiguration.Username, _captchaConfiguration.Password);
        }

        private void CheckBalance()
        {
            var saldo = _client.GetBalance();

            if (saldo / 100 < 1)
                throw new CaptchaNotSolvedException("Balance less than US$1");
        }

        public void ResolveCaptcha(Func<Stream> captureImage, Func<string, bool> callback)
        {            
            CheckBalance();

            var result = Policy
                .Handle<System.Exception>()                
                .WaitAndRetry(_captchaConfiguration.RetryBy, i => TimeSpan.FromSeconds(_captchaConfiguration.SecondsBetweenRetry))
                .ExecuteAndCapture(() => {
                    var image = captureImage();
                    var captchaResult = _client.Decode(image, _captchaConfiguration.Timeout);

                    if (captchaResult == null || !captchaResult.Solved)
                    {
                        _client.Report(captchaResult);
                        logService.Log(LogLevel.Trace, $"Unable to resolve ReCaptcha");                        
                        throw new CaptchaNotSolvedException();
                    }

                    logService.Log(LogLevel.Trace, $"Captcha was resolved: {captchaResult.Text}");

                    if (!callback(captchaResult.Text))
                    {
                        _client.Report(captchaResult);
                        throw new CaptchaNotSolvedException();
                    }
                });

            if(result.Outcome != OutcomeType.Successful)
                throw result.FinalException;
        }

        public void ResolveCaptcha(Stream image, Func<string, bool> callback)
        {
            CheckBalance();

            var captchaResult = _client.Decode(image, _captchaConfiguration.Timeout);

            if (captchaResult == null || !captchaResult.Solved)
            {
                _client.Report(captchaResult);
                logService.Log(LogLevel.Trace, $"Unable to resolve ReCaptcha");
                callback(string.Empty);
                return;
            }

            logService.Log(LogLevel.Trace, $"Captcha was resolved: {captchaResult.Text}");

            var resolved = callback(captchaResult.Text);

            if (!resolved)
                _client.Report(captchaResult);
        }

        public void ResolveReCaptcha(string siteUrl, string siteKey, Func<string, bool> onSucces, Action<string> onFaliure)
        {
            CheckBalance();
            var cancellationToken = new CancellationTokenSource();
            cancellationToken.CancelAfter(TimeSpan.FromMinutes(10));

            Task.Factory.StartNew(() =>
            {
                var cprms = new
                {
                    googlekey = siteKey,
                    pageurl = siteUrl,
                    min_score = 0.3
                };

                try
                {
                    _client = new DeathByCaptcha.HttpClient(_captchaConfiguration.Username, _captchaConfiguration.Password);
                    var cto = Convert.ToInt32(_captchaConfiguration.RecaptchaTimeout);

                    var result = Policy
                    .Handle<System.Exception>()
                    .WaitAndRetry(_captchaConfiguration.RecaptchaRetryBy, i => TimeSpan.FromSeconds(_captchaConfiguration.RecaptchSecondsBetweenRetry))
                    .ExecuteAndCapture(() =>
                    {
                        var resp = _client.Decode(cto, new Hashtable() { { "type", 4 }, { "token_params", JsonConvert.SerializeObject(cprms) } });

                        if (resp == null || !onSucces(resp.Text))
                            logService.LogWarning($"Unable to resolve ReCaptcha. Captcha id {resp?.Id}");

                        logService.Log(LogLevel.Trace, $"Recaptcha was resolved: {resp.Text}");
                    });

                    if (result.Outcome != OutcomeType.Successful)
                        throw result.FinalException;                    
                }
                catch (System.Exception ex)
                {
                    logService.Log(LogLevel.Information, $"Params: {cprms}, TimeOut: {_captchaConfiguration.RecaptchaTimeout}");
                    onFaliure(ex.ToString());
                }
            }, cancellationToken.Token).Wait();
        }

        public void ResolveReCaptchaV3(string siteUrl, string googlekey, string action, Func<string, bool> onSucces, Action<string> onFaliure)
        {
            CheckBalance();
            var cancellationToken = new CancellationTokenSource();
            cancellationToken.CancelAfter(TimeSpan.FromMinutes(10));

            Task.Factory.StartNew(() =>
            {
                var url = new Uri(siteUrl);
                var pageurl = url.AbsoluteUri.Substring(0, url.AbsoluteUri.Length - url.PathAndQuery.Length);

                var cprms = new
                {
                    proxy = "http://user:password@127.0.0.1:1234",
                    proxytype = "HTTP",
                    googlekey,
                    pageurl = siteUrl,
                    action,
                    min_score = 0.3
                };

                try
                {
                    _client = new DeathByCaptcha.HttpClient(_captchaConfiguration.Username, _captchaConfiguration.Password);
                    var cto = Convert.ToInt32(_captchaConfiguration.RecaptchaTimeout);
                    var resp = _client.Decode(cto, new Hashtable()
                    {
                        {"type", 5 },
                        {"token_params", JsonConvert.SerializeObject(cprms) }
                    });

                    if (resp == null || !onSucces(resp.Text))
                        logService.LogWarning($"Unable to resolve ReCaptcha. Captcha id {resp?.Id}");

                    logService.Log(LogLevel.Trace, $"Recaptcha was resolved: {resp.Text}");
                }
                catch (System.Exception ex)
                {
                    logService.Log(LogLevel.Information, $"Params: {cprms}, TimeOut: {_captchaConfiguration.RecaptchaTimeout}");
                    onFaliure(ex.ToString());
                }
            }, cancellationToken.Token).Wait();
        }

        public void ResolveHCaptcha(string siteUrl, string siteKey, Func<string, bool> onSucces, Action<string> onFaliure)
        {
            throw new NotImplementedException();
        }
    }
}
