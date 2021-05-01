using System.Threading.Tasks;

using Up4All.WebCrawler.Domain.Models;

namespace Up4All.WebCrawler.Framework.Contracts
{
    public interface IProcess     
    {
        IChromeService ChromeService { get; }
        void Start();
        void End();
        void Process(Metadata metadata);
    }
}
