using System;
using System.IO;
using NeuronDocumentSync.Interfaces;

namespace NeuronDocumentSync.Models
{
    public class GeneralConfig : IGeneralConfig
    {
        private readonly string _commonAppDirectory;
        private readonly IConfigProcessor _cfgProcessor;
        private const int DefaultBreakMinutesValue = 10;

        public GeneralConfig(IConfigProcessor cfgProcessor)
        {
            _cfgProcessor = cfgProcessor;
            _commonAppDirectory = Environment.GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData) + @"\NeuronDocumentSync\";
            try
            {
                if (!Directory.Exists(_commonAppDirectory))
                {
                    Directory.CreateDirectory(_commonAppDirectory);
                }
            }
            catch (Exception)
            {
                _commonAppDirectory = Directory.GetCurrentDirectory();
            }
            

            TempDirectoryPath = 
                 _commonAppDirectory;
            IsServiceTimeEnable = false;
            ServiceTimeStart = new TimeSpan(0);
            ServiceTimeEnd = ServiceTimeStart;
            SyncOperationBreakValue = 1000 * 60 * DefaultBreakMinutesValue;
        }
        public string TempDirectoryPath { get; set; }
        public string AppDirectoryPath {
            get { return _commonAppDirectory; }}

        public int SyncOperationBreakValue { get; set; }
        public bool IsServiceTimeEnable { get; set; }
        public TimeSpan ServiceTimeStart { get; set; }
        public TimeSpan ServiceTimeEnd { get; set; }
        public string WebImportUrl { get; set; }

        public void Load()
        {
            _cfgProcessor.LoadGeneralConfig(this);
        }

        public bool Save()
        {
            return _cfgProcessor.SaveGeneralConfig(this);
        }
    }
}