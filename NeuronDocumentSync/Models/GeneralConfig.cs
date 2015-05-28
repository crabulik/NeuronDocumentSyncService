﻿using System;
using NeuronDocumentSync.Interfaces;

namespace NeuronDocumentSync.Models
{
    public class GeneralConfig : IGeneralConfig
    {
        private readonly string _commonAppDirectory; 
            
        public GeneralConfig()
        {
            _commonAppDirectory = Environment.GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData) + @"\NeuronDocumentSync\";
            TempDirectoryPath = 
                 _commonAppDirectory;
        }
        public string TempDirectoryPath { get; set; }
        public string AppDirectoryPath {
            get { return _commonAppDirectory; }}
    }
}