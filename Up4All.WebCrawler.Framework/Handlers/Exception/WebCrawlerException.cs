namespace Up4All.WebCrawler.Framework.Handlers.Exception
{
    public class WebCrawlerException<T> : System.Exception
    {
        public WebCrawlerException(System.Exception exception) : base("WebCrawlerException", exception)
        {

        }
    }
}