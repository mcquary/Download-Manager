using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Downloader;
using Microsoft.Win32;

namespace DownloadManager
{
    public class TemplateViewModel : PropertyChangedBase, IShell
    {
        readonly IWindowManager _windowManager;
        IEventAggregator _eventAgg;
        private string _filePathToAdd;
        private string _urlBox;
        private DownloadManagerHerder _downloader = new DownloadManagerHerder();
        private SingleDownload _selectedSingle;

        
        public TemplateViewModel(IWindowManager windowManager, IEventAggregator eventAgg)
        {
            string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            _windowManager = windowManager;
            _eventAgg = eventAgg;
            _filePathToAdd = Path.Combine(pathUser, "Downloads");
        }

        public string UrlBox
        {
            get { return _urlBox; }
            set
            {
                _urlBox = value;
                NotifyOfPropertyChange(() => UrlBox);
            }
        }

        public string FilePathBox
        {
            get { return _filePathToAdd; }
            set 
            { 
                _filePathToAdd = value;
                NotifyOfPropertyChange(() => FilePathBox);
            }
        }

        public void StopDownloads()
        {
            foreach (var item in _downloader.Downloads)
                item.StopDownload();
        }

        public void ResetDownloads()
        {
            foreach (var item in _downloader.Downloads)
                item.ResetDownload();
        }
        
        public void AddDownload()
        {
            _downloader.AddDownload(_urlBox, FilePathBox);

            UrlBox = string.Empty;
        }

        public BindingList<SingleDownload> Downloads
        {
            get { return _downloader.Downloads; }
        }

        public void StopSingle()
        {
            if (_selectedSingle == null)
                return;

            _selectedSingle.StopDownload();
            
            _selectedSingle = null;
        }

        public void RemoveSingle()
        {
            _downloader.RemoveDownload(_selectedSingle);
        }
        
        public SingleDownload SelectedDownloads
        {
            get { return _selectedSingle; }
            set
            {
                _selectedSingle = value;
                NotifyOfPropertyChange(() => SelectedDownloads);
            }
        }

        public void Download()
        {
            try
            {
                foreach (var download in _downloader.Downloads)
                {
                    download.Download();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }


    }
}
