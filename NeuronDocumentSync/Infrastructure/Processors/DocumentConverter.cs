using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop.Word;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;
using NeuronDocumentSync.Resources;

namespace NeuronDocumentSync.Infrastructure.Processors
{
    public class DocumentConverter
    {
        private const string ZipTempFileName = "temp.zip";
        private const string ZipTempFolder = @"temp_zip";
        private const string WordTempFileName = "temp.doc";
        private const string PdfTempFileName = "temp.pdf";
        private readonly IGeneralConfig _cfg;
        private readonly INeuronLogger _logger;
        public DocumentConverter(IGeneralConfig cfg, INeuronLogger logger)
        {
            _cfg = cfg;
            _logger = logger;
        }
        public ExportServiceDocument Convert(NeuronDocument doc)
        {
            if (doc.DocumentType == ExportDocumentsType.NotSet)
            {
                return null;
            }

            byte[] tmpPdf = null;
            var tmpImageList = new List<ExportedImage>();

            switch (doc.DocumentType)
            {
                case ExportDocumentsType.WordDoc:
                    {
                        if (doc.DocumentData != null)
                        {
                            tmpPdf = GetPdfFromWord(doc.DocumentData);
                        }
                        break;
                    }
                case ExportDocumentsType.Image:
                    {
                        if (doc.DocumentData != null)
                        {
                            tmpImageList.Add(GetImageFromArray(doc.DocumentData, doc.FileName));
                        }
                        break;
                    }
                case ExportDocumentsType.DocPlusImage:
                    {
                        if (doc.DocumentAdditionalData != null)
                        {
                            tmpImageList.Add(GetImageFromArray(doc.DocumentAdditionalData, Path.ChangeExtension(doc.FileName, "png")));
                        }
                        if (doc.DocumentData != null)
                        {
                            tmpPdf = GetPdfFromWord(doc.DocumentData);
                        }
                        break;
                    }
                case ExportDocumentsType.ZipImages:
                    {
                        if (doc.DocumentData != null)
                        {
                            tmpImageList.AddRange(GetImagesFromZip(doc.DocumentData));
                        }
                        break;
                    }
                case ExportDocumentsType.DocPlusZipImages:
                    {
                        if (doc.DocumentAdditionalData != null)
                        {
                            tmpImageList.AddRange(GetImagesFromZip(doc.DocumentAdditionalData));
                        }
                        if (doc.DocumentData != null)
                        {
                            tmpPdf = GetPdfFromWord(doc.DocumentData);
                        }
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if ((tmpPdf == null) && (tmpImageList.Count == 0))
            {
                return null;
            }
            return(new ExportServiceDocument
            {
                CreatDate = doc.CreatDate,
                DeliveryEMail = doc.DeliveryEMail,
                DeliveryPhone = doc.DeliveryPhone,
                Name = doc.Name,
                NeuronDbDocumentId = doc.ID,
                PdfFileData = tmpPdf,
                ImagesInterpretation = tmpImageList
            });
        }

        public ExportedImage GetImageFromArray(byte[] source, string fileName)
        {
            var tmpFilePath = Path.Combine(_cfg.TempDirectoryPath, fileName);
            if (File.Exists(tmpFilePath))
            {
                File.Delete(tmpFilePath);
            }
            File.WriteAllBytes(tmpFilePath, source);

            var mime = MimeMapping.GetMimeMapping(tmpFilePath);
            var result = new ExportedImage
            {
                FileName = fileName,
                ImageData = source,
                MimeType = mime
            };

            File.Delete(tmpFilePath);
            return result;
        }

        public List<ExportedImage> GetImagesFromZip(byte[] source)
        {
            var tmpFilePath = Path.Combine(_cfg.TempDirectoryPath, ZipTempFileName);
            if (File.Exists(tmpFilePath))
            {
                File.Delete(tmpFilePath);
            }
            File.WriteAllBytes(tmpFilePath, source);

            var tmpFolderPath = Path.Combine(_cfg.TempDirectoryPath, ZipTempFolder);

            if (Directory.Exists(tmpFolderPath))
            {
                Directory.Delete(tmpFolderPath, true);
            }

            ZipFile.ExtractToDirectory(tmpFilePath, tmpFolderPath);

            var result = new List<ExportedImage>();
            foreach (var fileName in Directory.EnumerateFiles(tmpFolderPath)
                .Where(file => file.ToLower().EndsWith("png") 
                    || file.ToLower().EndsWith("jpg")
                    || file.ToLower().EndsWith("jpeg")))
            {
                var tmpImage = new ExportedImage
                {
                    FileName = Path.GetFileName(fileName),
                    MimeType = MimeMapping.GetMimeMapping(fileName),
                    ImageData = File.ReadAllBytes(fileName)
                };
                result.Add(tmpImage);
            }
            


            File.Delete(tmpFilePath);
            Directory.Delete(tmpFolderPath, true);
            return result;
        }

        public Image GetImageFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            try
            {
                return CreateNonIndexedImage(filePath);
            }
            catch (Exception ex)
            {
                _logger.AddLog(MainMessages.rs_SomeProblemsWithImageFile, ex, EventLogEntryType.Error);
                return null;
            }
            
        }

        public static Image CreateNonIndexedImage(string path)
        {
            using (var sourceImage = Image.FromFile(path))
            {
                var targetImage = new Bitmap(sourceImage.Width, sourceImage.Height,
                  PixelFormat.Format32bppArgb);
                using (var canvas = Graphics.FromImage(targetImage))
                {
                    canvas.DrawImageUnscaled(sourceImage, 0, 0);
                }
                return targetImage;
            }
        } 

        public byte[] GetPdfFromWord(byte[] wordDoc)
        {
            try
            {
                var tmpWordFilePath = Path.Combine(_cfg.TempDirectoryPath, WordTempFileName);
                if (File.Exists(tmpWordFilePath))
                {
                    File.Delete(tmpWordFilePath);
                }
                File.WriteAllBytes(tmpWordFilePath, wordDoc);

                var tmpPdfFolderPath = Path.Combine(_cfg.TempDirectoryPath, PdfTempFileName);

                if (File.Exists(tmpPdfFolderPath))
                {
                    File.Delete(tmpPdfFolderPath);
                }

                byte[] result;
                var appWord = new Application();
                var wordDocument = appWord.Documents.Open(tmpWordFilePath);
                try
                {
                    wordDocument.ExportAsFixedFormat(tmpPdfFolderPath, WdExportFormat.wdExportFormatPDF);
                    result = File.ReadAllBytes(tmpPdfFolderPath);
                }
                finally
                {
                    wordDocument.Close();
                    appWord.Quit();
                }
                               
                File.Delete(tmpWordFilePath);
                File.Delete(tmpPdfFolderPath);
                return result;
            }
            catch (Exception ex)
            {
                _logger.AddLog(MainMessages.rs_SomeProblemsWithImageFile, ex, EventLogEntryType.Error);
                return null;
            }
        }
    }
}