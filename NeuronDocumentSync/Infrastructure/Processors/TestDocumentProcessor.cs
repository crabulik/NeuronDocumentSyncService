using System;
using System.Drawing.Imaging;
using System.IO;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;

namespace NeuronDocumentSync.Infrastructure.Processors
{
    public class TestDocumentProcessor: INeuronDocumentProcessor
    {
        private readonly string _exportDirectory = @"I:\Test";
        private readonly DocumentConverter _converter;

        public TestDocumentProcessor(DocumentConverter converter)
        {
            _converter = converter;
        }
        public bool ProcessDocument(NeuronDocument document)
        {
            var exportDoc = _converter.Convert(document);
            if (exportDoc != null)
            {
                var outputPath = Path.Combine(_exportDirectory, exportDoc.NeuronDbDocumentId.ToString());
                Directory.CreateDirectory(outputPath);
                if (exportDoc.PdfFileData != null)
                  File.WriteAllBytes(Path.Combine(outputPath, exportDoc.CreatDate.ToShortDateString() + ".pdf"), exportDoc.PdfFileData);

                foreach (var img in exportDoc.ImagesInterpretation)
                {
                    img.Save(Path.Combine(outputPath, Guid.NewGuid() + ".png"), System.Drawing.Imaging.ImageFormat.Png);
                }

                return true;
            }

            return false;
        }
    }
}