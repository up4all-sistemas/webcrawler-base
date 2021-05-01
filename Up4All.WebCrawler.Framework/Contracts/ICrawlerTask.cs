using System.Threading.Tasks;

using Up4All.WebCrawler.Framework.Entities;

namespace Up4All.WebCrawler.Framework.Contracts
{
    public interface ICrawlerTask
    {
        int TaskId { get; }

        string TaskName { get; }
        
        Task RunAsync(Context context);

        Task SaveTask(Context context);
    }
}
