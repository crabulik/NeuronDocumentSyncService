using NeuronDocumentSync.Models;

namespace NeuronDocumentSync.Interfaces
{
    public interface INeuronDocumentProcessor
    {
        bool ProcessDocument(NeuronDocument document);
    }
}