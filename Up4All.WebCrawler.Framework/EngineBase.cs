using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OpenQA.Selenium;

using Polly;
using Polly.Timeout;

using System;
using System.Collections.Generic;
using System.Linq;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.WebCrawler.Domain.Models;
using Up4All.WebCrawler.Framework.Contracts;
using Up4All.WebCrawler.Framework.Options;
using Up4All.WebCrawler.Framework.Services;

using Context = Up4All.WebCrawler.Framework.Entities.Context;
using Exception = System.Exception;

namespace Up4All.WebCrawler.Framework
{
    public partial class EngineBase :  IProcess, IDisposable
    {
        public IChromeService ChromeService { get; }
        private readonly ICollection<ICrawlerTask> _taskFromSource;
        private readonly IMessageBusConsumer _busClient;
        private readonly CrawlerOptions _options;
        

        protected readonly ILogger<EngineBase> LogService;
        protected readonly Context Context;

        protected string BotName { get; }

        protected readonly IServiceProvider ServiceProvider;

        private bool disposedValue = false;

        public EngineBase(IChromeService chromeService,
                             ILogger<EngineBase> logService,                             
                             Context context,
                             IMessageBusConsumer busClient,
                             IOptions<CrawlerOptions> opts,
                             IServiceProvider serviceProvider)
        {
            ChromeService = chromeService;            
            _taskFromSource = new List<ICrawlerTask>();
            _busClient = busClient;
            _options = opts.Value;
            LogService = logService;

            BotName = _options.BotName;
            ServiceProvider = serviceProvider;
            Context = context;
            Context.BotName = BotName;

            _taskFromSource = serviceProvider.GetServices<ICrawlerTask>().ToList();
        }

        public void Process(Metadata metadata)
        {
            try
            {
                if (!ProcessIsHealthly())
                    return;

                PrepareToExecute(metadata);
                ExecuteAsync();
            }
            catch (Exception ex)
            {
                Notify(ex.Message).Wait();
            }
        }

        protected void PrepareToExecute(Metadata metadata)
        {
            Context.ClearContext();

            Context.SetContext(metadata);
            var taskToRun = _taskFromSource.FirstOrDefault(x => x.TaskId == Context.Metadata.TaskId);
            Context.StartTask(taskToRun);
        }

        public void ExecuteAsync()
        {            
            ExecuteTask(Context.Task);
        }

        private void ExecuteTask(ICrawlerTask task)
        {
            var tries = _options.Retries ?? 2;

            try
            {
                var hndExc = Policy
                    .Handle<Exception>()
                    .WaitAndRetry(tries, (i) => { return TimeSpan.FromMilliseconds(new Random().Next(500, 2000) * i); }
                        , (ex, ts) =>
                        {
                            LogService.LogWarning($"Task failed, retrying in {ts}");
                            ChromeService.ResetBrowser();
                        });

                var timeout = _options.TaskTimeout ?? 600;
                var hndTout = Policy.Timeout(TimeSpan.FromMinutes(timeout), TimeoutStrategy.Pessimistic);
                var wrap = hndExc.Wrap(hndTout);

                var result = wrap.ExecuteAndCapture(() => {
                    LogService.LogInformation($"Starting task [{task.TaskId}] {task.TaskName}");
                    task.RunAsync(Context).ConfigureAwait(false).GetAwaiter().GetResult();
                });

                if (result.Outcome == OutcomeType.Failure)
                    throw result.FinalException;
            }
            catch (TimeoutRejectedException ex)
            {
                LogService.LogError(ex, "Task timeout excedeed.");
                ShutDown();
            }
            catch (WebDriverTimeoutException ex)
            {
                LogService.LogError(ex, "Task source unavailable");
                ShutDown();
            }
            catch (UnavailableSourceException ex)
            {
                LogService.LogError(ex, "Task source unavailable");
                ShutDown();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex, "Error in task execution");
                ShutDown();
            }
            finally
            {
                task.SaveTask(Context);
                if (Context.Shutdown) Dispose();
            }
        }

        protected void ShutDown()
        {
            Context.ShutdownBot();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _busClient.Close();
                    ChromeService.Driver.Quit();
                    Context.ClearContext();
                    Environment.ExitCode = 0;

                    LogService.LogWarning("Shutdown Bot");
                    LogService.LogWarning("Closing process");
                    
                    ProcessManager.Closing.Set();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}