using Newtonsoft.Json.Linq;

namespace Up4All.WebCrawler.Domain.Models
{
    public class Metadata
    {
        public JObject Data { get; private set; }

        public int TaskId { get; private set; }

        public Metadata()
        {
        }

        public Metadata(object data, int taskId) : this()
        {
            SetData(data);
            SetTaskId(taskId);
        }

        public void SetData<T>(T data)
        {
            Data = JObject.FromObject(data);
        }

        public T GetData<T>()
        {
            if (Data == null)
                return default(T);

            return Data.ToObject<T>();
        }

        public void SetTaskId(int id)
        {
            TaskId = id;
        }
    }
}
