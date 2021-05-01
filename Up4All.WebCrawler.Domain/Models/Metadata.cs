using Newtonsoft.Json.Linq;

namespace Up4All.WebCrawler.Domain.Models
{
    public class Metadata
    {
        public object Data { get; set; }

        public int TaskId { get; set; }

        public T GetData<T>()
        {
            return ((JObject)Data).ToObject<T>();
        }
    }
}
