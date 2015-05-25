using System;
using System.Collections.Generic;
using System.Drawing;
using NeuronDocumentSync.Enums;

namespace NeuronDocumentSync.Models
{
    public class ExportServiceDocument
    {
        public int NeuronDbDocumentId { get; set; }

        public string Name { get; set; }

        public string DeliveryEMail { get; set; }

        public string DeliveryPhone { get; set; }

        public DateTime CreatDate { get; set; }

        public List<Image> ImagesInterpretation { get; set; }

        public byte[] PdfFileData { get; set; }

        public ExportServiceDocument()
        {
        }
    }
}