using System.Drawing;
using System.IO;
using System.Threading.Tasks;

using Up4All.WebCrawler.Framework.Entities;

namespace Up4All.WebCrawler.Framework.Contracts
{
    public interface ITaskService
    {        
        Stream CreateEvidenceAsync(Context context);

        Stream CreateEvidenceAsyncNoFP(Context context);

        Stream CreateEvidenceEntireScreen(Context context, int delayTime = 2000);

        string CleanAccents(string text);

        Stream GeneratePdf(Image img);

        bool CompareName(string sourceName, string contextName);

        Task SaveAsync(Context context);
    }
}
