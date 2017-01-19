using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Timers;

namespace Downloader
{
    public class SingleDownload : PropertyNotifier, IDisposable
    {
        private NetworkHelper _netHelper;
        private System.Timers.Timer _secondTimer;
        private int _bytesDownTotalSpeed = 0;
        private BasicFileInfo _info;
        private string _outputMessage;
        private Exception _exceptionInfo;
        private DownloadableFileState _currentState;
        private BackgroundWorker downloadWorker;
        private int _progress = 0;
        private Int64 partialFileSizeBytes;
        private bool _doAutoResume = false;
        private Int64 _totalFileSizeBytes = 0;
        private bool _overwriteOnDownloadIfSameName = false;
        private long _totalTimeTicks = 0;

        public event ProgressChangedEventHandler StatusUpdated;

        public SingleDownload()
        {
            _currentState = DownloadableFileState.Pending;
            _netHelper = new NetworkHelper();
        }

        public SingleDownload(BasicFileInfo fileInfo)
        {
            _info = fileInfo;
            _currentState = DownloadableFileState.Pending;
            //_netHelper = new NetworkHelper();
            if (_info.PartSizeBytes > 0)
                partialFileSizeBytes = _info.PartSizeBytes;

            _totalFileSizeBytes = long.Parse(_info.TotalSizeBytes);
        }

        public SingleDownload(string url, string saveFilePath)
        {
            _info = ActiveFileDownload.GetDownloadMetaObject(url, saveFilePath); //new BasicFileInfo(url, saveFilePath);
            _currentState = DownloadableFileState.Pending;
            //_netHelper = new NetworkHelper();
        }

        public void ResetDownload()
        {
            if (_info.CurrentState == DownloadableFileState.Started || _info.CurrentState == DownloadableFileState.InProgress)
            {
                CancelDownload();
                var count = 100;
                while (_info.CurrentState != DownloadableFileState.Cancelled) { Thread.Sleep(100); count--; if (count == 0) break; }
            }

            try
            {
                if (File.Exists(_info.TemporaryPath))
                    File.Delete(_info.TemporaryPath);
                _info.Refresh();
                partialFileSizeBytes = _info.PartSizeBytes;
                OnPropertyChanged(() => TotalDownloadedFormatted);
                OnPropertyChanged(() => TotalFileSizeFormatted);
                OnPropertyChanged(() => OutputMessage);
                OnPropertyChanged(() => CurrentState);
            }
            catch (Exception ex)
            {
                _exceptionInfo = ex;
                SetState(DownloadableFileState.Error);
            }
        }

        public BasicFileInfo FileInfo
        {
            get { return _info; }
        }

        public Int64 TotalFileSizeBytes
        {
            get
            {
                return _totalFileSizeBytes;
            }

            private set
            {
                _totalFileSizeBytes = value;
                OnPropertyChanged(() => TotalFileSizeBytes);
                OnPropertyChanged(() => TotalFileSizeFormatted);
            }
        }

        public string TotalDownloadedFormatted
        {
            get { return FormatFileSize(partialFileSizeBytes); }
        }

        public string TotalTimeString
        {
            get { return FormatTime(_totalTimeTicks); }
        }

        public string TotalFileSizeFormatted
        {
            get { return FormatFileSize(_totalFileSizeBytes); }
        }

        public string CurrentState
        {
            get
            {
                switch (_info.CurrentState)
                {
                    case DownloadableFileState.InProgress:
                        return "In Progress...";
                    default:
                        return _info.CurrentState.ToString() + "...";
                }
            }
        }

        public DownloadableFileState CurrentStateEnum
        {
            get
            {
                return _info.CurrentState;
            }
        }

        public string OutputMessage
        {
            get { return _outputMessage; }
            set
            {
                _outputMessage = value;
            }
        }

        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged(() => Progress);
            }
        }

        public void StopDownload()
        {
            if (downloadWorker != null)
            {
                downloadWorker.CancelAsync();
            }

            SetState(DownloadableFileState.Stopped);
            _info.Refresh();
            partialFileSizeBytes = _info.PartSizeBytes;
            OnPropertyChanged(() => CurrentState);
        }

        public bool AutoResume
        {
            get { return _doAutoResume; }
            set { _doAutoResume = value; }
        }

        public string Url
        {
            get { return _info.Url; }
        }


        private void Timer_Tick(object sender, ElapsedEventArgs e)
        {
            OnPropertyChanged(() => CurrentState);
            OnPropertyChanged(() => TotalDownloadedFormatted);
            if (_bytesDownTotalSpeed > 0)
            {
                OutputMessage = Math.Round((_bytesDownTotalSpeed / 1000d), 2).ToString() + " KB/s";
            }
            else
                OutputMessage = "0 KB/s";
            OnPropertyChanged(() => OutputMessage);
            _bytesDownTotalSpeed = 0;
        }

        private static string FormatFileSize(Int64 size, int decimalPlaces)
        {

            string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            Double formattedSize = size;
            int sizeIndex = 0;
            while (formattedSize >= 1000 && sizeIndex < sizes.Length)
            {
                formattedSize /= 1000;
                sizeIndex += 1;
            }
            return Math.Round(formattedSize, decimalPlaces) + sizes[sizeIndex];
        }

        private static string FormatTime(long ticks)
        {
            var testTime = new TimeSpan(ticks);
            return testTime.ToString();
        }

        private static string FormatFileSize(Int64 fileSizeBytes)
        {
            return FormatFileSize(fileSizeBytes, 2);
        }

        public void SetState(DownloadableFileState state)
        {
            try
            {
                switch (state)
                {
                    default:
                        _info.CurrentState = state;
                        break;
                }
                if (StatusUpdated != null)
                    StatusUpdated(null, new ProgressChangedEventArgs(_progress, null));
                OnPropertyChanged(() => CurrentState);
            }
            catch (Exception ex)
            {

            }
        }

        public void Download()
        {
            //CheckEntryIntoLog();
            StartTimer();
            DoDownloadBW();
        }

        private void StartTimer()
        {
            _secondTimer = new System.Timers.Timer(1000);
            _secondTimer.Elapsed += new ElapsedEventHandler(Timer_Tick);
            _secondTimer.Enabled = true;
        }

        private void DoDownloadBW()
        {

            if (downloadWorker != null && (_info.CurrentState == DownloadableFileState.Started || _info.CurrentState == DownloadableFileState.InProgress))
            {
                downloadWorker.CancelAsync();
                downloadWorker.Dispose();
                downloadWorker = null;
            }

            downloadWorker = new BackgroundWorker();
            downloadWorker.WorkerReportsProgress = true;
            downloadWorker.WorkerSupportsCancellation = true;
            downloadWorker.DoWork += new DoWorkEventHandler(BW_DoWork);
            downloadWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(downloadWorker_RunWorkerCompleted);
            downloadWorker.ProgressChanged += new ProgressChangedEventHandler(downloadWorker_ProgressChanged);
            downloadWorker.RunWorkerAsync();
        }

        public Exception InnerException
        {
            get { return _exceptionInfo; }
        }

        private void downloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                SetState(DownloadableFileState.Cancelled);
            }

            KillTimer();
            downloadWorker.DoWork -= BW_DoWork;
            downloadWorker.ProgressChanged -= downloadWorker_ProgressChanged;
            OnPropertyChanged(() => TotalDownloadedFormatted);
            OnPropertyChanged(() => TotalTimeString);
            OnPropertyChanged(() => CurrentState);
        }

        private void KillTimer()
        {
            if (_secondTimer != null)
            {
                _secondTimer.Enabled = false;
                _secondTimer.Elapsed -= Timer_Tick;
                _secondTimer = null;
            }
        }

        private void downloadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;

            if (StatusUpdated != null)
                StatusUpdated(sender, e);
        }

        private FileStream GetFileStream(string path, bool DoAppend)
        {
            if (DoAppend)
                return new FileStream(path, FileMode.Append, FileAccess.Write);
            else
                return new FileStream(path, FileMode.Create, FileAccess.Write);
        }

        private void BW_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;

            SetState(DownloadableFileState.Started);

            if (_doAutoResume)
            {
                while (_doAutoResume && _info.CurrentState != DownloadableFileState.Completed
                    && _info.CurrentState != DownloadableFileState.Stopped
                    && _info.CurrentState != DownloadableFileState.Cancelled
                    && !worker.CancellationPending)
                {
                    try
                    {
                        DoDownload(ref worker);
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() == typeof(WebException))
                        {

                        }
                        _exceptionInfo = ex;
                        SetState(DownloadableFileState.Error);
                        return;
                    }
                }
            }
            else
            {
                try
                {
                    DoDownload(ref worker);
                }
                catch (Exception ex)
                {
                    _exceptionInfo = ex;
                    SetState(DownloadableFileState.Error);
                }
            }
        }

        public bool OverwriteOnDownloadIfSameName
        {
            get { return _overwriteOnDownloadIfSameName; }
            set
            {
                _overwriteOnDownloadIfSameName = value;
                OnPropertyChanged(() => OverwriteOnDownloadIfSameName);
            }
        }

        private Int64 GetTotalDownloadFileSize()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_info.Url);
            req.Method = "HEAD";
            req.KeepAlive = false;
            using (HttpWebResponse resp = (HttpWebResponse)(req.GetResponse()))
            {
                var size = resp.ContentLength;
                resp.Close();
                return size;
            }
        }

        public void CancelDownload()
        {
            if (downloadWorker != null)
                downloadWorker.CancelAsync();

            OnPropertyChanged(() => CurrentState);

        }

        private void DoDownload(ref BackgroundWorker worker)
        {

            Stream response = null;
            FileStream downloadPart = null;
            long startTicks = 0;
            long stopTicks = 0;
            try
            {

                startTicks = DateTime.Now.Ticks;
                var startPointInt = _info.PartSizeBytes;
                var webRequest = (HttpWebRequest)WebRequest.Create(_info.Url);
                webRequest.Credentials = CredentialCache.DefaultCredentials;

                TotalFileSizeBytes = GetTotalDownloadFileSize();
                _info.TotalSizeBytes = TotalFileSizeBytes.ToString();
                _info.Refresh();
                webRequest.AddRange(startPointInt);
                webRequest.KeepAlive = false;
                var webResponse = (HttpWebResponse)webRequest.GetResponse();

                try
                {
                    Int64 fileSize = webResponse.ContentLength;

                    if (startPointInt >= fileSize)
                    {

                    }
                    else
                    {

                        using (response = webResponse.GetResponseStream())
                        {
                            Thread.CurrentThread.Priority = ThreadPriority.Lowest;

                            using (downloadPart = GetFileStream(_info.TemporaryPath, startPointInt != 0))
                            {
                                int bytesSize = 0;
                                partialFileSizeBytes = startPointInt;
                                byte[] downBuffer = new byte[32768];

                                SetState(DownloadableFileState.InProgress);
                                while ((bytesSize = response.Read(downBuffer, 0, downBuffer.Length)) > 0 && !worker.CancellationPending)
                                {
                                    _bytesDownTotalSpeed += bytesSize;

                                    downloadPart.Write(downBuffer, 0, bytesSize);

                                    if (_currentState == DownloadableFileState.Cancelled)
                                        break;
                                    if (_currentState == DownloadableFileState.Stopped)
                                        break;
                                    int percentageDone = 0;
                                    partialFileSizeBytes += bytesSize;
                                    if (_totalFileSizeBytes > 0)
                                        percentageDone = Convert.ToInt32((partialFileSizeBytes / (float)_totalFileSizeBytes) * 100);
                                    worker.ReportProgress(percentageDone);
                                }
                                response.Close();
                            }
                            downloadPart.Close();
                        }
                    }
                    webResponse.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (webResponse != null)
                        webResponse.Close();
                }

                if (!worker.CancellationPending)
                {
                    var count = 1;
                    string fileNameOnly = Path.GetFileNameWithoutExtension(_info.DestinationPath);
                    string extension = Path.GetExtension(_info.DestinationPath);
                    string path = Path.GetDirectoryName(_info.DestinationPath);
                    string newFullPath = _info.DestinationPath;

                    if (OverwriteOnDownloadIfSameName)
                    {
                        if (File.Exists(newFullPath))
                            File.Delete(newFullPath);
                    }
                    else
                    {
                        while (File.Exists(newFullPath))
                        {
                            string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                            newFullPath = Path.Combine(path, tempFileName + extension);
                        }
                    }

                    _info.DestinationPath = newFullPath;
                    File.Move(_info.TemporaryPath, _info.DestinationPath);
                    MetaDataManager.RemoveFromDatFile(_info);
                    SetState(DownloadableFileState.Completed);

                }
            }
            catch (Exception ex)
            {
                _exceptionInfo = ex;
                SetState(DownloadableFileState.Error);
            }
            finally
            {
                if (response != null)
                    response.Close();
                if (downloadPart != null)
                    downloadPart.Close();
            }

            stopTicks = DateTime.Now.Ticks;

            _totalTimeTicks += stopTicks - startTicks;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {

                if (downloadWorker != null)
                {
                    downloadWorker.Dispose();
                    downloadWorker = null;
                }

                if (_secondTimer != null)
                {
                    _secondTimer.Dispose();
                    _secondTimer = null;
                }
            }
        }

        ~SingleDownload()
        {
            Dispose(false);
        }

    }
}
