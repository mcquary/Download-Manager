using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Downloader
{
    public class ActiveFileDownload
    {
       
        private static BasicFileInfo GetNewDownloadMetaObject(string url, string saveFilePath)
        {
            BasicFileInfo result = new BasicFileInfo();

            result.Extension = ".part";
            result.Url = url;
            result.TempName = Guid.NewGuid().ToString() + result.Extension;
            var tempPath = System.IO.Path.GetTempPath();

            if (url.Contains("?"))
            {
                var index = url.IndexOf("?");
                var strippedUrl = url.Substring(0, index);
                url = strippedUrl;
            }

            var destinationName = url.Split("/"[0])[url.Split("/"[0]).Length - 1];
            var isFolder = string.IsNullOrEmpty(Path.GetExtension(saveFilePath));

            if (isFolder)
                result.DestinationPath = Path.Combine(saveFilePath, destinationName);
            else
                result.DestinationPath = saveFilePath;

            if (string.IsNullOrEmpty(tempPath))
            {
                if (isFolder)
                    tempPath = saveFilePath;
                else
                {
                    var fileName = Path.GetFileName(saveFilePath);
                    tempPath = saveFilePath.Replace(fileName, "");
                }
            }

            result.TemporaryPath = Path.Combine(tempPath, result.TempName);

            if (File.Exists(result.TemporaryPath))
            {
                int size = 0;
                var file = new FileInfo(result.TemporaryPath);
                int.TryParse(file.Length.ToString(), out size);
                result.PartSizeBytes = size;
                result.StartPointBytes = size;
            }
            else
            {
                result.TotalSizeBytes = "0";
                result.PartSizeBytes = 0;
                result.StartPointBytes = 0;
            }
            result.CurrentState = DownloadableFileState.Pending;

            return result;
        }

        public static BasicFileInfo GetDownloadMetaObject(string url, string saveFilePath)
        {
            BasicFileInfo result;
            result = GetNewDownloadMetaObject(url, saveFilePath);

            return result;
        }
    }
}
