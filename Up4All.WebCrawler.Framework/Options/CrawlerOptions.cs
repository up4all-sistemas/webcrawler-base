using System;
using System.Collections.Generic;
using System.Text;

namespace Up4All.WebCrawler.Framework.Options
{
    public class CrawlerOptions
    {
        public CrawlerChromeOptions Chrome { get; set; }

        public string BotName { get; set; }

        public int? JobTask { get; set; }

        public int? Retries { get; set; }

        public int? TaskTimeout { get; set; }

        public CrawlerOptions()
        {
            Chrome = new CrawlerChromeOptions();
        }
    }

    public class CrawlerChromeOptions
    {
        public int? PageLoadTimeOut { get; set; }

        public bool HeadlessMode { get; set; }

        public string ChromeBinaryPath { get; set; }

        public string ChromeDriverPath { get; set; }

        public ProxyConfiguration Proxy { get; set; }

        public CrawlerChromeOptions()
        {
            Proxy = new ProxyConfiguration();
        }
    }

    public class ProxyConfiguration
    {
        public bool Enabled { get; set; }

        public bool Authenticated { get; set; }

        public string ProxyUser { get; set; }

        public string ProxyPass { get; set; }

        public string[] Ips { get; set; }
    }
}
