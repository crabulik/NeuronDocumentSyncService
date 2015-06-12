using System;
using System.Collections.Generic;
using System.Diagnostics;
using MSSManagers.FirebirdFacade;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.FbData;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;
using NeuronDocumentSync.Resources;

namespace NeuronDocumentSync.Infrastructure
{
    public class NeuronRepository
    {
        private FbDbConnectionConfig _dbConfig;
        private readonly INeuronLogger _logger;
        public NeuronRepository(FbDbConnectionConfig dbConfig, INeuronLogger logger)
        {
            SetDbConfig(dbConfig);
            _logger = logger;
        }

        public void SetDbConfig(FbDbConnectionConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public List<NeuronDocument> GetUnhandledDocumentsInfo()
        {
            var result = new List<NeuronDocument>();
            try
            {
                using (var facade = new FbFacade(true, _dbConfig.ToConnectionString()))
                using (var reader = facade.ExecuteReader(FbScripts.GetUnhandledDocs))
                {
                    if (reader.HasRows)
                    {
                        var idOrdinal = reader.GetOrdinal(DocumentsFields.Id);
                        var nameOrdinal = reader.GetOrdinal(DocumentsFields.Name);
                        var fileNameOrdinal = reader.GetOrdinal(DocumentsFields.FileName);
                        var mailOrdinal = reader.GetOrdinal(DocumentsFields.Mail);
                        var phoneOrdinal = reader.GetOrdinal(DocumentsFields.Phone);
                        var createDateOrdinal = reader.GetOrdinal(DocumentsFields.CreateDate);
                        var docTypeOrdinal = reader.GetOrdinal(DocumentsFields.DocumentType);

                        while (reader.Read())
                        {
                            var item = new NeuronDocument
                            {
                                ID = reader.GetInt32(idOrdinal),
                                Name = reader.GetString(nameOrdinal),
                                DeliveryEMail = reader.GetString(mailOrdinal),
                                FileName = reader.GetString(fileNameOrdinal),
                                DeliveryPhone = reader.GetString(phoneOrdinal),
                                CreatDate = reader.GetDateTime(createDateOrdinal),
                                DocumentType = Enum.IsDefined(typeof(ExportDocumentsType), reader.GetValue(docTypeOrdinal))
                                    ? (ExportDocumentsType)Enum.ToObject(typeof(ExportDocumentsType), reader.GetValue(docTypeOrdinal))
                                    : ExportDocumentsType.NotSet
                            };
                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.AddLog(MainMessages.rs_ErrorsInRepositoryGetUnhandledDocumentsInfo,
                    e,
                    EventLogEntryType.Error);
            }

            return result;
        }

        public bool FillDocumentData(NeuronDocument document)
        {
            if ((document != null) && (document.ID > 0))
            {
                using (var facade = new FbFacade(true, _dbConfig.ToConnectionString()))
                {
                    facade.Parameters.AddWithValue("@PNr", document.ID);
                    using (var reader = facade.ExecuteReader(FbScripts.GetDocData))
                    {
                        if (reader.HasRows)
                        {
                            var dataOrdinal = reader.GetOrdinal(DocumentsFields.Data);
                            var addDataOrdinal = reader.GetOrdinal(DocumentsFields.AdditionalData);
                            if (reader.Read())
                            {
                                document.DocumentData = (reader.IsDBNull(dataOrdinal))
                                    ? null
                                    : reader.GetFieldValue<Byte[]>(dataOrdinal);
                                document.DocumentAdditionalData = (reader.IsDBNull(addDataOrdinal))
                                    ? null
                                    : reader.GetFieldValue<Byte[]>(addDataOrdinal);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public void SetDocumentHandled(NeuronDocument document)
        {
            if ((document != null) && (document.ID > 0))
            {
                using (var facade = new FbFacade(true, _dbConfig.ToConnectionString()))
                {
                    if (facade.BeginWriteTransaction())
                    {
                        try
                        {
                            InsertToLog(document, facade, DbLogAction.DocumentHandled);

                            facade.Parameters.Clear();
                            facade.Parameters.AddWithValue("@PNr", document.ID);
                            facade.ExecuteNonQuery(FbScripts.SetHandled);

                            facade.CommitWriteTransaction();
                        }
                        catch (Exception ex)
                        {
                            _logger.AddLog(MainMessages.rs_ErrorsInRepository, ex, EventLogEntryType.Error);
                        }

                    }
                }
            }
        }

        public void SetDocumentUnhandleable(NeuronDocument document)
        {
            if ((document != null) && (document.ID > 0))
            {
                using (var facade = new FbFacade(true, _dbConfig.ToConnectionString()))
                {
                    if (facade.BeginWriteTransaction())
                    {
                        try
                        {
                            InsertToLog(document, facade, DbLogAction.DocumentUnhandlable);

                            facade.Parameters.Clear();
                            facade.Parameters.AddWithValue("@PNr", document.ID);
                            facade.ExecuteNonQuery(FbScripts.SetUnhandlable);

                            facade.CommitWriteTransaction();
                        }
                        catch (Exception ex)
                        {
                            _logger.AddLog(MainMessages.rs_ErrorsInRepository, ex, EventLogEntryType.Error);
                        }

                    }
                }
            }
        }

        private void InsertToLog(NeuronDocument document, FbFacade facade, DbLogAction action)
        {
            facade.Parameters.Clear();
            facade.Parameters.AddWithValue("@PNAME", document.Name);
            facade.Parameters.AddWithValue("@PCREATEDATE", document.CreatDate);
            facade.Parameters.AddWithValue("@PACTION", action);
            facade.Parameters.AddWithValue("@PDOCUMENT_ID", document.ID);
            facade.Parameters.AddWithValue("@PINFO_MESSAGE", document.Errors);
            facade.ExecuteNonQuery(FbScripts.InsertLog);
        }

        public void LogDocumentError(NeuronDocument document)
        {
            if ((document != null) && (document.ID > 0))
            {
                using (var facade = new FbFacade(true, _dbConfig.ToConnectionString()))
                {
                    if (facade.BeginWriteTransaction())
                    {
                        try
                        {
                            InsertToLog(document, facade, DbLogAction.DocumentGeneralError);

                            facade.CommitWriteTransaction();
                        }
                        catch (Exception ex)
                        {
                            _logger.AddLog(MainMessages.rs_ErrorsInRepository, ex, EventLogEntryType.Error);
                        }

                    }
                }
            }
        }

        public void DeleteDocument(int id)
        {

            using (var facade = new FbFacade(true, _dbConfig.ToConnectionString()))
            {
                if (facade.BeginWriteTransaction())
                {
                    try
                    {
                        facade.Parameters.Clear();
                        facade.Parameters.AddWithValue("@PNR", id);
                        facade.ExecuteNonQuery(FbScripts.DeleteDocument);

                        facade.CommitWriteTransaction();
                    }
                    catch (Exception ex)
                    {
                        _logger.AddLog(MainMessages.rs_ErrorsInRepository, ex, EventLogEntryType.Error);
                    }

                }
            }
        }
        public void DeleteHandledDocuments()
        {

            using (var facade = new FbFacade(true, _dbConfig.ToConnectionString()))
            {
                if (facade.BeginWriteTransaction())
                {
                    try
                    {
                        facade.Parameters.Clear();
                        facade.ExecuteNonQuery(FbScripts.DeleteHandledDocuments);

                        facade.CommitWriteTransaction();
                    }
                    catch (Exception ex)
                    {
                        _logger.AddLog(MainMessages.rs_ErrorsInRepository, ex, EventLogEntryType.Error);
                    }

                }
            }
        }
    }
}