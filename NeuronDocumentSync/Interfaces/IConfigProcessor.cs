using NeuronDocumentSync.Models;

namespace NeuronDocumentSync.Interfaces
{
    public interface IConfigProcessor
    {
        void LoadGeneralConfig(IGeneralConfig cfg);
        bool SaveGeneralConfig(IGeneralConfig cfg);
        void LoadFbDbConnectionConfig(FbDbConnectionConfig cfg);
        bool SaveFbDbConnectionConfig(FbDbConnectionConfig cfg);


    }
}