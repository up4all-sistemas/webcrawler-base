using Microsoft.Extensions.Logging;

using System;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.WebCrawler.Domain.Models;
using Up4All.WebCrawler.Framework.Contracts;
using Up4All.WebCrawler.Framework.Extensions.Strings;

using Task = System.Threading.Tasks.Task;

namespace Up4All.WebCrawler.Framework
{
    public partial class EngineBase : IProcess
    {
        public virtual void Start()
        {
            try
            {
                LogService.LogInformation($"Starting bot {BotName}");
                ChromeService.ConfigureWebBrowser();

                if (_options.JobTask.HasValue)
                {
                    LogService.LogInformation("Bot configured to execute a job");
                    var metadata = new Metadata();
                    metadata.SetTaskId(_options.JobTask.Value);

                    Process(metadata);
                    ShutDown();
                    Dispose();
                    return;
                }

                _busClient.RegisterHandler(data =>
                {
                    try
                    {
                        var metadata = data.GetBody<Metadata>(new Newtonsoft.Json.JsonSerializerSettings
                        {
                            ConstructorHandling = Newtonsoft.Json.ConstructorHandling.AllowNonPublicDefaultConstructor
                        });
                        Process(metadata);
                        return MessageReceivedStatusEnum.Completed;
                    }
                    catch (Exception ex)
                    {
                        LogService.LogError(ex.Message);
                        return MessageReceivedStatusEnum.Deadletter;
                    }
                },
                ex =>
                {
                    LogService.LogError(ex.Message);
                });
            }
            catch (Exception ex)
            {
                LogService.LogError(ex.Message, ex);
                ShutDown();
                Dispose();
                throw ex;
            }
        }

        public virtual void End()
        {
            LogService.LogInformation("Disconnecting...");
        }

        private bool ProcessIsHealthly()
        {
            var me = System.Diagnostics.Process.GetCurrentProcess();
            LogService.LogTrace($"{Environment.MachineName} | {BotName} | {me.WorkingSet64.SizeSuffix()}");
            return true;
        }

        private Task Notify(string message)
        {            
            return Task.CompletedTask;
        }
    }
}