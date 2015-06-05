using System;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Models;

namespace NeuronDocumentSync.Interfaces
{
    public interface ISyncCore
    {
        SyncCoreStatus GetStatus();

        void OnStart(string[] args);

        void OnStop();

        void UpdatePreferences(IGeneralConfig cfg, bool withRebootWorker = false);

        event EventHandler<ServiceStatusInfoArgs> StausChanged;
    }
}