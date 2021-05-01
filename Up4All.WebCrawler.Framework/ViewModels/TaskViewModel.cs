using System;

namespace Up4All.WebCrawler.Framework.ViewModels
{
    public class TaskViewModel
    {
        public Guid MetadataId { get; set; }
        public int TaskId { get; set; }
        public Guid CustomerId { get; set; }
        public int LoteId { get; set; }
    }
}
