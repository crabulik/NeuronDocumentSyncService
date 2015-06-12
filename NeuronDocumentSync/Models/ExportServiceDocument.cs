using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Resources;

namespace NeuronDocumentSync.Models
{
    public class ExportServiceDocument
    {

        public int NeuronDbDocumentId { get; set; }

        public string Name { get; set; }
        public string DeliveryEMail { get; set; }

        public string DeliveryPhone { get; set; }

        public DateTime CreatDate { get; set; }

        public List<ExportedImage> ImagesInterpretation { get; set; }

        public byte[] PdfFileData { get; set; }

        public ExportServiceDocument()
        {
        }
    }

    public class ExportedImage
    {

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public byte[] ImageData { get; set; }
    }
}