
using Up4All.WebCrawler.Domain.Models;
using Up4All.WebCrawler.Framework.Contracts;

namespace Up4All.WebCrawler.Framework.Entities
{
    public class Context
    {
        public string BotName { get; set; }

        public bool Shutdown { get; private set; }

        public bool Succesful { get; private set; }

        public Metadata Metadata { get; private set; }

        public TaskResult Result { get; private set; }

        public ICrawlerTask Task { get; private set; }

        public void ShutdownBot()
        {
            Shutdown = true;
        }

        public void SetContext(Metadata metadata)
        {
            Metadata = metadata;
            Result = new TaskResult();
        }

        public void StartTask(ICrawlerTask task)
        {
            Task = task;
        }

        public void ClearContext()
        {
            Task = null;
            Metadata = null;
            Result = null;
        }
    }


}
