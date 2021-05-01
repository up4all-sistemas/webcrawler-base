using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.Threading.Tasks;

using Up4All.WebCrawler.Framework.Contracts;
using Up4All.WebCrawler.Framework.Entities;

namespace Up4All.WebCrawler.Framework.Tasks
{
    public abstract class CrawlerTaskBase : ICrawlerTask
    {
        protected readonly IChromeService ChromeService;
        protected readonly ITaskService TaskService;
        protected readonly ILogger<CrawlerTaskBase> LogService;
        protected readonly IConfiguration Configuration;
        protected readonly ICaptchaSolver CaptchaSolver;

        public abstract int TaskId { get; }

        public abstract string TaskName { get; }

        protected CrawlerTaskBase(
            IChromeService chromeService,
            ITaskService taskService,
            ILogger<CrawlerTaskBase> logService,
            IConfiguration configuration,
            ICaptchaSolver captchaSolver)
        {
            ChromeService = chromeService;
            TaskService = taskService;
            LogService = logService;
            Configuration = configuration;
            CaptchaSolver = captchaSolver;
        }

        public abstract Task RunAsync(Context context);

        public Task SaveTask(Context context)
        {
            TaskService.SaveAsync(context);
            return Task.CompletedTask;
        }
    }
}