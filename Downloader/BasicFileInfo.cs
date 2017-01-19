using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace Downloader
{
    public enum DownloadableFileState
    { 
        Cancelled,
        Pending,
        Started,
        InProgress,
        Stopped,
        Completed,
        Error
    }

    public class BasicFileInfo
    {
        private string _tempName;
        private string _destinationPath;
        private string _fileName;
        private string _tempPath;
        private int _startPoint;
        private int _size;
        private string _url;
        private DownloadableFileState _state;
        private string _extension;
        private string _fileStreamPath;
        private string _totalSizeBytes;



        public string TempName { get { return _tempName; } set { _tempName = value; } }
        public string DestinationPath { get { return _destinationPath; } set { _destinationPath = value; } }
        public string TemporaryPath { get { return _fileStreamPath; } set { _fileStreamPath = value; } }
        public string FileName { get { return _fileName; } set { _fileName = value; } }
        public int StartPointBytes { get { return _startPoint; } set { _startPoint = value; } }
        public int PartSizeBytes { get { return _size; } set { _size = value; } }
        public string Url { get { return _url; } set { _url = value; } }
        public string Extension { get { return _extension; } set { _extension = value; } }
        public DownloadableFileState CurrentState{ get { return _state; } set { _state = value; } }
        public string TotalSizeBytes { get { return _totalSizeBytes; } set { _totalSizeBytes = value; } }

        public BasicFileInfo(string extension, string url, string destinationPath,
                string temporaryPath, string partSizeBytes, string startByteIndex,
                string totalSizeBytes, DownloadableFileState state)
        {
            Extension = extension;
            Url = url;
            DestinationPath = destinationPath;
            TemporaryPath = temporaryPath;
            PartSizeBytes = int.Parse(partSizeBytes);
            StartPointBytes = int.Parse(startByteIndex);
            TotalSizeBytes = totalSizeBytes;
            CurrentState = state;
        }

        public BasicFileInfo(string[] item)
        {
            Extension = ".kdfp";
            Url = item[0];
            DestinationPath = item[4];
            TemporaryPath = item[1];
            PartSizeBytes = int.Parse(item[2]);
            StartPointBytes = int.Parse(item[2]);
            TotalSizeBytes = item[3];
            CurrentState = DownloadableFileState.Pending;
        }

        public BasicFileInfo()
        {
            // TODO: Complete member initialization
        }

        public void Refresh()
        {
            if (File.Exists(this.TemporaryPath))
            {
                var file = new FileInfo(this.TemporaryPath);
                int.TryParse(file.Length.ToString(), out _size);
                _startPoint = _size;
            }
            MetaDataManager.AddOrUpdateItem(this);
        }
    }
}
