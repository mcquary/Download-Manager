using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Downloader
{
    static class MetaDataManager
    {
        static string _filePathLocation = System.IO.Path.GetTempPath();
        static string _datFileName = "downloader-todo.dat";
        static char _delimiter = '|';
        static List<string[]> _storedDownloads = new List<string[]>();

        static void WriteDatEntry(BasicFileInfo fileInfo)
        {
            string metaData = GetMetaString(fileInfo);
        }

        static void CreateMetaFile()
        { 
            var fullFilePath = Path.Combine(_filePathLocation, _datFileName);
            try
            {
                if (File.Exists(fullFilePath))
                {
                    File.Delete(fullFilePath);
                }

                using (var file = File.Create(fullFilePath))
                {
                    file.Close();
                }
            }
            catch (Exception)
            { 
            
            }
        }

        public static List<BasicFileInfo> LoadCurrentEntries()
        {
            var fullFilePath = Path.Combine(_filePathLocation, _datFileName);

            if (!File.Exists(fullFilePath))
                CreateMetaFile();
            using (var reader = new StreamReader(fullFilePath))
            { 
                string result;
                while ((result = reader.ReadLine()) != null)
                {
                    var fileInfo = GetMetaArray(result);
                    if (fileInfo == null)
                        continue;
                    _storedDownloads.Add(fileInfo);
                }
                reader.Close();
            }

            var returnSet = new List<BasicFileInfo>();
            foreach (var item in _storedDownloads)
            {
                returnSet.Add(new BasicFileInfo(item));
            }

            return returnSet;
        }

        private static void SaveToFile()
        {
            var fullFilePath = Path.Combine(_filePathLocation, _datFileName);

            if (!File.Exists(fullFilePath))
                CreateMetaFile();
            using (var writer = new StreamWriter(fullFilePath))
            {
                foreach(var item in _storedDownloads)
                {
                    if (item == null)
                        continue;
                    string result = GetMetaString(item);
                    writer.WriteLine(result);
                }
                writer.Close();
            }
        }

        static string GetMetaString(string[] info)
        {
            //var url = info.Url;
            //var filestreamPath = info.FileStreamPath;
            //var currentSize = info.CurrentSizeBytes;
            //var totalSize = info.TotalSizeBytes;
            //var destinationPath = info.DestinationPath;
            return info[0] + _delimiter + info[1] + _delimiter + info[2] + _delimiter + info[3] + _delimiter + info[4];
        }

        static string GetMetaString(BasicFileInfo info)
        {
            var url = info.Url;
            var filestreamPath = info.TemporaryPath;
            var currentSize = info.PartSizeBytes;
            var totalSize = info.TotalSizeBytes;
            var destinationPath = info.DestinationPath;

            return url + _delimiter + filestreamPath + _delimiter + currentSize + _delimiter + totalSize + _delimiter + destinationPath;
        }

        static string[] GetMetaArray(string delimitedString)
        {
            var _array = new string[5];
            var meta = delimitedString.Split(_delimiter);
            if (meta.Count() == 5)
            {
                for (byte i = 0; i < 5; i++)
                    _array[i] = meta[i];
            }
            else
                return null;

            return _array;
        }

        public static void RemoveFromDatFile(BasicFileInfo basicFileInfo)
        {
            for (int i = 0; i < _storedDownloads.Count; i++)
            {
                if (_storedDownloads[i][(int)DatFileColumns.FileStreamPath] == basicFileInfo.TemporaryPath)
                {
                    _storedDownloads.Remove(_storedDownloads[i]);
                    break;
                }
            }
            SaveToFile();
        }

        public static void AddDownloadLog(BasicFileInfo info)
        {
            AddOrUpdateItem(info);
        }
         
        static List<string[]> FetchDownloads()
        {
            return new List<string[]>();
        }

        public enum DatFileColumns
        {
            Url,
            FileStreamPath,
            CurrentSizeBytes,
            FullSizeBytes,
            DestinationFilePath
        }

        public class TempFileMetaData
        {
            string[] _array;
            public TempFileMetaData(string url, string fullSizeBytes, string currentSizeBytes, string saveFilePath, string fileStreamPath)
            {
                _array = new string[5];
                _array[(int)DatFileColumns.Url] = url;
                _array[(int)DatFileColumns.FullSizeBytes] = fullSizeBytes;
                _array[(int)DatFileColumns.CurrentSizeBytes] = currentSizeBytes;
                _array[(int)DatFileColumns.DestinationFilePath] = saveFilePath;
                _array[(int)DatFileColumns.FileStreamPath] = fileStreamPath;
            }

           
        }
        public static string GetProperty(this string[] download, DatFileColumns column)
        {
            if (download== null)
                return null;
            return download[(int)column];
        }

        public static void AddOrUpdateItem(BasicFileInfo basicFileInfo)
        {
            var found = false;
            for (int i = 0; i < _storedDownloads.Count; i++)
            {
                if (_storedDownloads[i][(int)DatFileColumns.FileStreamPath] == basicFileInfo.TemporaryPath)
                {
                    _storedDownloads[i] = GetMetaArray(GetMetaString(basicFileInfo));
                    found = true;
                    break;
                }
            }
            if (!found)
                _storedDownloads.Add(GetMetaArray(GetMetaString(basicFileInfo)));
            SaveToFile();
        }


    }
}
