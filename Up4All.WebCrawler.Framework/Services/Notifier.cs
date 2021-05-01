using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Up4All.WebCrawler.Framework.Services
{
    public static class Notifier
    {
        public static async Task Notify(string message)
        {
            try
            {
                message += $"\nMachine Name: {Environment.MachineName}";
                using (var httpClient = new HttpClient())
                {
                    var url = $"https://api.telegram.org/bot909176132:AAGgpxb7Fgt3AJzy0xbaGKrsSJn0RRj6qak/sendMessage?chat_id=-1001155093320&parse_mode=MARKDOWN&text={message}";
                    await httpClient.GetAsync(url);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
