using System;
using System.ComponentModel;
using System.IO;

namespace Downloader
{
    public class DownloadManagerHerder : IDisposable
    {
        private delegate void UpdateProgessCallback(Int64 BytesRead, Int64 TotalBytes);
        private BindingList<SingleDownload> _pendingDownloads = new BindingList<SingleDownload>();
        
        public DownloadManagerHerder()
        {
            GetCurrentOpenDownloads();
        }

        ~DownloadManagerHerder()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
			{
				foreach (IDisposable containedItem in _pendingDownloads)
					containedItem.Dispose();   
			}
        }

        public void AddDownload(string url, string filePath)
        {
            var newFile = ActiveFileDownload.GetDownloadMetaObject(url, filePath);
            MetaDataManager.AddDownloadLog(newFile);
            _pendingDownloads.Add(new SingleDownload(newFile));
        }

        public void StartAllDownloads()
        {
            foreach (var item in _pendingDownloads)
            {
                item.Download();
            }
        }

        public BindingList<SingleDownload> Downloads
        {
            get { return _pendingDownloads; }
        }

        public void StopAllDownloads()
        {
            foreach (var item in _pendingDownloads)
                item.StopDownload();
        }

        private void GetCurrentOpenDownloads()
        {
            var result = MetaDataManager.LoadCurrentEntries();
            foreach (var item in result)
            {
                var download = new SingleDownload(item);
                _pendingDownloads.Add(download);
            }
        }

        public void RemoveDownload(SingleDownload _selectedSingle)
        {
            _pendingDownloads.Remove(_selectedSingle);
            MetaDataManager.RemoveFromDatFile(_selectedSingle.FileInfo);
        }
    }
}
