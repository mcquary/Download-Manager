using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Downloader
{
    class Program
    {
        static void Main(string[] args)
        {
            var testDownloadManager = new DownloadManagerHerder();
            testDownloadManager.AddDownload("[URL STRING]", "[FILE PATH]");
            testDownloadManager.StartAllDownloads();
        }
    }
}
