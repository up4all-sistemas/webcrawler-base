using System;
using System.Collections.Generic;
using System.Text;

using Up4All.WebCrawler.Framework.Util;

namespace Up4All.WebCrawler.Framework.Entities.Enums
{
    public class TaskResultEnum : Enumeration
    {
        public static TaskResultEnum None = new TaskResultEnum(0, "None");
        public static TaskResultEnum Done = new TaskResultEnum(1, "Done");
        public static TaskResultEnum Error = new TaskResultEnum(2, "Error");
        public static TaskResultEnum Unavailable = new TaskResultEnum(3, "Unavailable");

        public TaskResultEnum(int id, string name) : base(id, name)
        {
        }
    }
}
