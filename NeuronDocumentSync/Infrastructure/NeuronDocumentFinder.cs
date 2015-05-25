

using System;
using System.Collections.Generic;
using System.Threading;
using MSSManagers.FirebirdFacade;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.FbData;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;

namespace NeuronDocumentSync.Infrastructure
{
    public class NeuronDocumentFinder
    {
        private readonly FbDbConnectionConfig _dbConfig;
        private readonly CancellationToken _cancellationToken;
        private readonly INeuronLogger _logger;
        public NeuronDocumentFinder(FbDbConnectionConfig dbConfig, CancellationToken cancellationToken,
            INeuronLogger logger)
        {
            _dbConfig = dbConfig;
            _cancellationToken = cancellationToken;
            _logger = logger;
        }

        public List<NeuronDocument> Execute()
        {
            var result = new List<NeuronDocument>();
            using (var facade = new FbFacade(true, _dbConfig.ToConnectionString()))
            using (var reader = facade.ExecuteReader(FbScripts.GetUnhandledDocs))
            {
                if (reader.HasRows)
                {
                    var idOrdinal = reader.GetOrdinal(DocumentsFields.Id);
                    var nameOrdinal = reader.GetOrdinal(DocumentsFields.Name);
                    var mailOrdinal = reader.GetOrdinal(DocumentsFields.Mail);
                    var phoneOrdinal = reader.GetOrdinal(DocumentsFields.Phone);
                    var createDateOrdinal = reader.GetOrdinal(DocumentsFields.CreateDate);
                    var docTypeOrdinal = reader.GetOrdinal(DocumentsFields.DocumentType);
                    var dataOrdinal = reader.GetOrdinal(DocumentsFields.Data);
                    var addDataOrdinal = reader.GetOrdinal(DocumentsFields.AdditionalData);
                    while (reader.Read())
                    {
                        if (_cancellationToken.IsCancellationRequested)
                        {
                            _logger.Log("Find operation canceled");
                            return result;
                        }


                        var item = new NeuronDocument
                        {
                            ID = reader.GetInt32(idOrdinal),
                            Name = reader.GetString(nameOrdinal),
                            DeliveryEMail = reader.GetString(mailOrdinal),
                            DeliveryPhone = reader.GetString(phoneOrdinal),
                            CreatDate = reader.GetDateTime(createDateOrdinal),
                            DocumentType = Enum.IsDefined(typeof(ExportDocumentsType), reader.GetValue(docTypeOrdinal))
                                ? (ExportDocumentsType)Enum.ToObject(typeof(ExportDocumentsType), reader.GetValue(docTypeOrdinal))
                                : ExportDocumentsType.NotSet,
                            DocumentData = (reader.IsDBNull(dataOrdinal))
                                ? null
                                : reader.GetFieldValue<Byte[]>(dataOrdinal),
                            DocumentAdditionalData = (reader.IsDBNull(addDataOrdinal))
                                ? null
                                : reader.GetFieldValue<Byte[]>(addDataOrdinal),
                        };
                        result.Add(item);
                    }
                }
            }

            return result;
        }
    }
}
