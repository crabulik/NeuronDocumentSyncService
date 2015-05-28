namespace NeuronDocumentSync.Interfaces
{
    public interface IGeneralConfig
    {
        string TempDirectoryPath { get; set; }
        string AppDirectoryPath { get; }
    }
}