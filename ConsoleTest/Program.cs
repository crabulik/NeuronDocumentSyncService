using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Infrastructure;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;
using Ninject;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var kernel = new StandardKernel(new TestBindings());

            var list = new List<NeuronDocument>();

            list.Add(new NeuronDocument
            {
                ID = 1,
                DocumentType = ExportDocumentsType.WordDoc,
                DocumentData = File.ReadAllBytes(@"I:\Test\4.docx"),
                CreatDate = DateTime.Now,
                Name = "1"
            });

            list.Add(new NeuronDocument
            {
                ID = 2,
                DocumentType = ExportDocumentsType.Image,
                DocumentData = File.ReadAllBytes(@"I:\Test\3.png"),
                CreatDate = DateTime.Now,
                Name = "2"
            });

            list.Add(new NeuronDocument
            {
                ID = 3,
                DocumentType = ExportDocumentsType.DocPlusImage,
                DocumentData = File.ReadAllBytes(@"I:\Test\4.docx"),
                DocumentAdditionalData = File.ReadAllBytes(@"I:\Test\3.png"),
                CreatDate = DateTime.Now,
                Name = "3"
            });

            list.Add(new NeuronDocument
            {
                ID = 4,
                DocumentType = ExportDocumentsType.DocPlusZipImages,
                DocumentData = File.ReadAllBytes(@"I:\Test\4.docx"),
                DocumentAdditionalData = File.ReadAllBytes(@"I:\Test\1.zip"),
                CreatDate = DateTime.Now,
                Name = "4"
            });

            list.Add(new NeuronDocument
            {
                ID = 5,
                DocumentType = ExportDocumentsType.ZipImages,
                DocumentData = File.ReadAllBytes(@"I:\Test\2.zip"),
                CreatDate = DateTime.Now,
                Name = "5"
            });

            var processor = kernel.Get<INeuronDocumentProcessor>();
            foreach (var doc in list)
            {
                processor.ProcessDocument(doc);
            }
        }
    }
}
