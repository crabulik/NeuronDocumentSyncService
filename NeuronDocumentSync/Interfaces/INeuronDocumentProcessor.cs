using System.Collections.Generic;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Models;

namespace NeuronDocumentSync.Interfaces
{
    public interface INeuronDocumentProcessor
    {
        NeuronDocumentProcessorResult ProcessDocument(NeuronDocument document);

        NeuronDocumentProcessorResult PublishDocuments();
    }
}