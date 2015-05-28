namespace NeuronDocumentSyncService
{
    public interface IServiceManager
    {
        bool IsServiceInstalled();
        bool IsServiceRunning();
        void InstallService();
        void UninstallService();
        void StartService();
        void StopService();
    }
}