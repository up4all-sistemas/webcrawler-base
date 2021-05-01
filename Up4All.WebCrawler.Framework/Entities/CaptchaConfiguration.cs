namespace Up4All.WebCrawler.Framework.Entities
{
    public class CaptchaConfiguration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Timeout { get; set; }
        public int RetryBy { get; set; }
        public int SecondsBetweenRetry { get; set; }
        public int RecaptchaTimeout { get; set; }
        public int RecaptchaRetryBy { get; set; }
        public int RecaptchSecondsBetweenRetry { get; set; }
        public bool Use2Capctha { get; set; }
        public string TwoCaptchaApiKey { get; set; }
    }
}
