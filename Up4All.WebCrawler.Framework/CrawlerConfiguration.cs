using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Net.Http;

using Up4All.Framework.MessageBus.Abstractions.Configurations;
using Up4All.Framework.MessageBus.RabbitMQ;
using Up4All.WebCrawler.Framework.ApiClients;
using Up4All.WebCrawler.Framework.ApiClients.Mocks;
using Up4All.WebCrawler.Framework.CaptchaSolvers;
using Up4All.WebCrawler.Framework.Contracts;
using Up4All.WebCrawler.Framework.Entities;
using Up4All.WebCrawler.Framework.Options;
using Up4All.WebCrawler.Framework.Services;
using Up4All.WebCrawler.Framework.Tasks;

namespace Up4All.WebCrawler.Framework
{
    public static class CrawlerConfiguration
    {
        public static IServiceCollection AddCrawlerServiceBase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging((builder) => builder.SetMinimumLevel(LogLevel.Trace));
            services.AddSingleton<Context>();            
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddSingleton<IChromeService, ChromeService>();
            services.AddSingleton<HttpClient>();
            services.AddSingleton(configuration);
            services.AddSingleton<IProcess, EngineBase>();
            services.Configure<CrawlerOptions>(o => configuration.GetSection(nameof(CrawlerOptions)).Bind(o));
            return services;
        }

        public static IServiceCollection AddTask<T>(this IServiceCollection services) where T : CrawlerTaskBase
        {
            services.AddSingleton<ICrawlerTask, T>();
            return services;
        }

        #region Captcha
        public static IServiceCollection AddCaptchaServicesDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            var capConfig = GetCaptchaConfiguration(configuration);
            services.AddSingleton(capConfig);

            if(capConfig.Use2Capctha)            
                services.AddSingleton<ICaptchaSolver, TwoCaptchaSolver>();
            else
                services.AddSingleton<ICaptchaSolver, DeadByCaptchaSolver>();

            services.AddSingleton<IImageService, ImageService>();

            var prxyCfg = GetProxyConfiguration(configuration);
            services.AddSingleton(prxyCfg);

            return services;
        }

        public static IServiceCollection AddCaptchaServicesDependenciesAsMock(this IServiceCollection services, IConfiguration configuration)
        {
            var capConfig = GetCaptchaConfiguration(configuration);
            services.AddSingleton(capConfig);

            if (capConfig.Use2Capctha)
                services.AddSingleton<ICaptchaSolver, TwoCaptchaSolver>();
            else
                services.AddSingleton<ICaptchaSolver, DeadByCaptchaSolver>();

            services.AddSingleton<IImageService, ImageService>();

            var prxyCfg = GetProxyConfigurationAsMock(configuration);
            services.AddSingleton(prxyCfg);

            return services;
        }

        private static ProxyConfiguration GetProxyConfiguration(IConfiguration config)
        {
            return new ProxyConfiguration
            {
                Enabled = config.GetValue("Proxy:Enabled", false),
                Ips = config.GetSection("Proxy:Ips").Get<string[]>(),
                Authenticated = config.GetValue("Proxy:Authenticated", false),
                ProxyUser = config.GetValue<string>("Proxy:ProxyUser"),
                ProxyPass = config.GetValue<string>("Proxy:ProxyPass"),
            };
        }

        private static ProxyConfiguration GetProxyConfigurationAsMock(IConfiguration config)
        {
            return new ProxyConfiguration
            {
                Enabled = config.GetValue("Proxy:Enabled", false),
                Ips = config.GetSection("Proxy:Ips").Get<string[]>(),
                Authenticated = config.GetValue("Proxy:Authenticated", false),
                ProxyUser = config.GetValue<string>("Proxy:ProxyUser"),
                ProxyPass = config.GetValue<string>("Proxy:ProxyPass"),
            };
        }

        private static CaptchaConfiguration GetCaptchaConfiguration(IConfiguration configuration)
        {
            return new CaptchaConfiguration
            {
                Username = configuration.GetValue<string>("Captcha:Username"),
                Password = configuration.GetValue<string>("Captcha:Password"),
                RecaptchaRetryBy = configuration.GetValue("Captcha:RecaptchaRetryBy", 1),
                RecaptchaTimeout = configuration.GetValue("Captcha:RecaptchaTimeout", 300),
                RecaptchSecondsBetweenRetry = configuration.GetValue("Captcha:RecaptchSecondsBetweenRetry", 3),
                RetryBy = configuration.GetValue("Captcha:RetryBy", 1),
                SecondsBetweenRetry = configuration.GetValue("Captcha:SecondsBetweenRetry", 3),
                Timeout = configuration.GetValue("Captcha:Timeout", 180),
                Use2Capctha = configuration.GetValue("Captcha:Use2Captcha", false),
                TwoCaptchaApiKey = configuration.GetValue<string>("Captcha:TwoCaptchaApiKey")
            };
        }

        private static CaptchaConfiguration GetCaptchaConfigurationAsMock(IConfiguration configuration)
        {
            return new CaptchaConfiguration
            {
                Username = configuration.GetValue<string>("Captcha:Username"),
                Password = configuration.GetValue<string>("Captcha:Password"),
                RecaptchaRetryBy = configuration.GetValue("Captcha:RecaptchaRetryBy", 1),
                RecaptchaTimeout = configuration.GetValue("Captcha:RecaptchaTimeout", 300),
                RecaptchSecondsBetweenRetry = configuration.GetValue("Captcha:RecaptchSecondsBetweenRetry", 3),
                RetryBy = configuration.GetValue("Captcha:RetryBy", 1),
                SecondsBetweenRetry = configuration.GetValue("Captcha:SecondsBetweenRetry", 3),
                Timeout = configuration.GetValue("Captcha:Timeout", 180),
                Use2Capctha = configuration.GetValue("Captcha:Use2Captcha", false),
                TwoCaptchaApiKey = configuration.GetValue<string>("Captcha:TwoCaptchaApiKey")
            };
        }
        #endregion

        public static IServiceCollection AddCrawlerServicesDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessageBusTopicClient<RabbitMQTopicClient>(configuration);
            services.AddMessageBusSubscribeClient<RabbitMQSubscribeClient>(configuration);
            services.AddMessageBusQueueClient<RabbitMQQueueClient>(configuration);
            services.AddSingleton<ITaskService, TaskService>();
            return services;
        }

        public static IServiceCollection AddCrawlerServicesDependenciesAsMock(this IServiceCollection services)
        {
            services.AddMessageBusTopicClientMocked<RabbitMQTopicClientMocked>();
            services.AddMessageBusSubscribeClientMocked<RabbitMQSubscribeClientMocked>();
            services.AddMessageBusQueueClientMocked<RabbitMQQueueClientMocked>();
            services.AddSingleton<ITaskService, TaskServiceMock>();
            return services;
        }
    }
}
