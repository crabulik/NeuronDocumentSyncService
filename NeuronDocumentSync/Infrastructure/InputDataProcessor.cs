using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;

namespace NeuronDocumentSync.Infrastructure
{
    public class InputDataProcessor
    {
        private readonly GeneralConfig _cfg;
        private readonly CancellationToken _cancellationToken;
        private readonly INeuronLogger _logger;
        public InputDataProcessor(GeneralConfig cfg, CancellationToken cancellationToken,
            INeuronLogger logger)
        {
            _cfg = cfg;
            _cancellationToken = cancellationToken;
            _logger = logger;
        }

        public List<ExportServiceDocument> GetExportServiceDocuments(List<NeuronDocument> source)
        {
            var result = new List<ExportServiceDocument>();

            foreach (var neuronDocument in source)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    _logger.AddLog("Input Data Processor operation canceled");
                    return result;
                }

                if (neuronDocument.DocumentType == ExportDocumentsType.NotSet)
                {
                    break;
                }

                byte[] tmpPdf = null;
                var tmpImageList = new List<Image>();

                switch (neuronDocument.DocumentType)
                {
                    case ExportDocumentsType.WordDoc:
                        break;
                    case ExportDocumentsType.Image:
                    {
                        if (neuronDocument.DocumentData != null)
                        {
                            tmpImageList.Add(GetImageFromArray(neuronDocument.DocumentData));
                        }
                        break;
                    }
                    case ExportDocumentsType.DocPlusImage:
                    {
                        if (neuronDocument.DocumentAdditionalData != null)
                        {
                            tmpImageList.Add(GetImageFromArray(neuronDocument.DocumentAdditionalData));
                        }
                        break;
                    }
                    case ExportDocumentsType.ZipImages:
                        break;
                    case ExportDocumentsType.DocPlusZipImages:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                result.Add(new ExportServiceDocument
                {
                    CreatDate = neuronDocument.CreatDate,
                    DeliveryEMail = neuronDocument.DeliveryEMail,
                    DeliveryPhone = neuronDocument.DeliveryPhone,
                    Name = neuronDocument.Name,
                    NeuronDbDocumentId = neuronDocument.ID,
                    PdfFileData = tmpPdf,
                    ImagesInterpretation = tmpImageList
                });
            }
            return result;
        }

        public Image GetImageFromArray(byte[] source)
        {
            var ms = new MemoryStream(source);
            return Image.FromStream(ms);
        }
    }
}