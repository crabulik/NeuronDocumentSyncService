using System;
using NeuronDocumentSync.Interfaces;

namespace NeuronDocumentSync.Models
{
    public class GeneralConfig : IGeneralConfig
    {
        private readonly string _commonAppDirectory;
        private readonly IConfigProcessor _cfgProcessor;

        public GeneralConfig(IConfigProcessor cfgProcessor)
        {
            _cfgProcessor = cfgProcessor;
            _commonAppDirectory = Environment.GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData) + @"\NeuronDocumentSync\";
            TempDirectoryPath = 
                 _commonAppDirectory;
        }
        public string TempDirectoryPath { get; set; }
        public string AppDirectoryPath {
            get { return _commonAppDirectory; }}

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