using System;
using NeuronDocumentSync.Enums;

namespace NeuronDocumentSync.Models
{
    public class NeuronDocument
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string DeliveryEMail { get; set; }

        public string DeliveryPhone { get; set; }

        public DateTime CreatDate { get; set; }
        
        public ExportDocumentsType DocumentType { get; set; }

        public byte[] DocumentData { get; set; }

        public byte[] DocumentAdditionalData { get; set; }

        public string Errors { get; set; }
    }
}