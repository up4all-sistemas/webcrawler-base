using System;
using System.Collections.Generic;
using System.Text;

using Up4All.WebCrawler.Framework.Entities.Enums;

namespace Up4All.WebCrawler.Framework.Entities
{
    public class TaskResult
    {
        public DateTime StartedAt { get; private set; }

        public DateTime? StoppedAt { get; private set; }

        public TaskResultEnum Status { get; private set; }

        public string Details { get; private set; }

        public TaskResult()
        {
            StartedAt = DateTime.UtcNow;
            Status = TaskResultEnum.None;
        }

        public void SetAsDone()
        {
            StoppedAt = DateTime.UtcNow;
            Status = TaskResultEnum.Done;
        }

        public void SetAsFailed()
        {
            StoppedAt = DateTime.UtcNow;
            Status = TaskResultEnum.Error;
        }

        public void SetAsUnavailable()
        {
            StoppedAt = DateTime.UtcNow;
            Status = TaskResultEnum.Unavailable;
        }

        public void SetDetails(string text)
        {
            Details = text;
        }
    }
}
