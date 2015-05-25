using System;

namespace NeuronDocumentSync.Models
{
    public class GeneralConfig
    {
        public GeneralConfig()
        {
            TempDirectoryPath = 
                Environment.GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData) + @"\NeuronDocumentSync\";
        }
        public string TempDirectoryPath { get; set; }
        
    }
}