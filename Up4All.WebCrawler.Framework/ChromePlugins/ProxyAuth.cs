
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Up4All.WebCrawler.Framework.Options;

namespace Up4All.WebCrawler.Framework.ChromePlugins
{
    public class ProxyAuth
    {
        private static string _manifest = "{\"version\": \"1.0.0\",\"manifest_version\": 2,\"name\": \"Chrome Proxy\",\"permissions\": [\"proxy\",\"tabs\",\"unlimitedStorage\",\"storage\",\"<all_urls>\",\"webRequest\",\"webRequestBlocking\"],\"background\": {\"scripts\": [ \"background.js\" ]},\"minimum_chrome_version\": \"22.0.0\"}";

        public static string GeneratePlugin(ProxyConfiguration proxy)
        {
            var ip = proxy.Ips[new Random().Next(0, proxy.Ips.Count() - 1)].Split(':');
            var host = ip[0];
            var port = ip[1];

            var js  = $"var config = {{mode: 'fixed_servers',rules:{{singleProxy:{{scheme: 'http',host: '{host}',port: {port}}},bypassList:['foobar.com']}}}};";
                js +=  "chrome.proxy.settings.set({ value: config, scope: 'regular' }, function () { });";
                js += $"function callbackFn(details) {{return {{authCredentials: {{username: '{proxy.ProxyUser}',password: '{proxy.ProxyPass}'}}}};}}";
                js += @"chrome.webRequest.onAuthRequired.addListener(callbackFn,{ urls: ['<all_urls>'] },['blocking']);";


            var tempPath = Path.GetTempPath();
            var manFile = Path.Combine(tempPath, "manifest.json");
            var jsFile = Path.Combine(tempPath, "background.js");

            File.WriteAllText(manFile, _manifest);
            File.WriteAllText(jsFile, js);

            var zipFile = Path.Combine(tempPath, "proxyauth.zip");

            if (File.Exists(zipFile))
                File.Delete(zipFile);

            using (var zip = ZipFile.Open(zipFile, ZipArchiveMode.Create))
            {   
                zip.CreateEntryFromFile(manFile, "manifest.json");
                zip.CreateEntryFromFile(jsFile, "background.js");
            }

            File.Delete(manFile);
            File.Delete(jsFile);

            return zipFile;
        }

    }
}
