using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;

namespace NeuronDocumentSync.Infrastructure.Processors
{
    public class TestDocumentProcessor//: INeuronDocumentProcessor
    {
        private readonly string _exportDirectory = @"I:\Test";
        private readonly DocumentConverter _converter;
        private readonly IGeneralConfig _cfg;

        public TestDocumentProcessor(DocumentConverter converter, IGeneralConfig cfg)
        {
            _converter = converter;
            _cfg = cfg;
        }
        public NeuronDocumentProcessorResult ProcessDocument(NeuronDocument document)
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
                    File.WriteAllBytes(Path.Combine(outputPath, img.FileName), img.ImageData);
                }


                var result = SendToWebIpTest(exportDoc);
                result.Wait();
                if (result.Result)
                    return NeuronDocumentProcessorResult.Success;
                return NeuronDocumentProcessorResult.Error;
            }

            return NeuronDocumentProcessorResult.Fail;
        }

        private async Task<bool> SendToWebIpTest(ExportServiceDocument document)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_cfg.WebImportUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsJsonAsync("api/import", document);
                var str = await response.Content.ReadAsStringAsync();
                Console.WriteLine(str);
                if (response.IsSuccessStatusCode)
                {
                    Uri url = response.Headers.Location;
                    //return true;
                }
            }

            return false;
        }
    }
}