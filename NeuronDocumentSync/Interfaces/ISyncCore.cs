using NeuronDocumentSync.Enums;

namespace NeuronDocumentSync.Interfaces
{
    public interface ISyncCore
    {
        SyncCoreStatus GetStatus();

        void OnStart(string[] args);

        void OnStop();
    }
}