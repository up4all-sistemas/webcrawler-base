using System;

namespace Up4All.WebCrawler.Framework.Extensions.Strings
{
    public static class MeasuresStringExtension
    {
        private static readonly string[] SizeSuffixes =
                 { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public  static string SizeSuffix(this long value, int decimalPlaces = 1)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }

            var i = 0;
            var dValue = (decimal)value;
            while (Math.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
        }

        public static double ConvertBytesToMegabytes(this long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }
    }
}
