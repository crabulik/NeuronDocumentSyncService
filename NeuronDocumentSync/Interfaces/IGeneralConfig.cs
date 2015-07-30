using System;

namespace NeuronDocumentSync.Interfaces
{
    public interface IGeneralConfig
    {
        string TempDirectoryPath { get; set; }
        string AppDirectoryPath { get; }

        int SyncOperationBreakValue { get; set; }

        bool IsServiceTimeEnable { get; set; }

        TimeSpan ServiceTimeStart { get; set; }

        TimeSpan ServiceTimeEnd { get; set; }

        string WebImportUrl { get; set; }

        string WebImportLogin { get; set; }

        string WebImportPass { get; set; }

        void Load();

        bool Save();
    }
}