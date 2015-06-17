using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;
using NeuronDocumentSync.Resources;

namespace NeuronDocumentSync.Infrastructure.Processors
{

    internal class WebApiSendResult
    {
        public NeuronDocumentProcessorResult Result;

        public string ErrorMessage;
    }

    public class AspNeuronDocumentProcessor : INeuronDocumentProcessor
    {
        

        private readonly DocumentConverter _converter;
        private readonly IGeneralConfig _cfg;
        private readonly INeuronLogger _logger;

        public AspNeuronDocumentProcessor(DocumentConverter converter, IGeneralConfig cfg, INeuronLogger logger)
        {
            _converter = converter;
            _cfg = cfg;
            _logger = logger;
        }
        public NeuronDocumentProcessorResult ProcessDocument(NeuronDocument document)
        {
            var exportDoc = _converter.Convert(document);
            if (exportDoc != null)
            {
                var result = SendDocumentToWebApi(exportDoc);
                result.Wait();
                document.Errors += result.Result.ErrorMessage;
                return result.Result.Result;
            }

            return NeuronDocumentProcessorResult.Fail;
        }

        public NeuronDocumentProcessorResult PublishDocuments()
        {
            var result = PublishDocumentsByWebApi();
            result.Wait();
            return result.Result.Result;
        }

        private async Task<WebApiSendResult> PublishDocumentsByWebApi()
        {
            var result = new WebApiSendResult();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_cfg.WebImportUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response;
                try
                {
                    response = await client.GetAsync("api/documentpublisher");
                }
                catch (Exception ex)
                {
                    _logger.AddLog(MainMessages.rs_AspNeuronDocumentPublisherError, ex, EventLogEntryType.Error);
                    result.ErrorMessage = MainMessages.rs_AspNeuronDocumentPublisherError;
                    result.Result = NeuronDocumentProcessorResult.Error;
                    return result;
                }


                if (response.IsSuccessStatusCode)
                {
                    var sended = await response.Content.ReadAsAsync<bool>();
                    result.Result = sended ? NeuronDocumentProcessorResult.Success : NeuronDocumentProcessorResult.Fail;
                }
                else
                {
                    result.Result = NeuronDocumentProcessorResult.Fail;
                    result.ErrorMessage = response.ReasonPhrase;
                    result.ErrorMessage = Environment.NewLine +
                        await response.Content.ReadAsStringAsync();
                    _logger.AddLog(MainMessages.rs_AspNeuronDocumentPublisherError +Environment.NewLine +
                        result.ErrorMessage);
                }
            }

            return result;
        }

        private async Task<WebApiSendResult> SendDocumentToWebApi(ExportServiceDocument document)
        {
            var result = new WebApiSendResult();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_cfg.WebImportUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response;
                try
                {
                    response = await client.PostAsJsonAsync("api/import", document);
                }
                catch (Exception ex)
                {
                    _logger.AddLog(MainMessages.rs_AspNeuronDocumentProcessorError, ex, EventLogEntryType.Error);
                    result.ErrorMessage = MainMessages.rs_AspNeuronDocumentProcessorError;
                    result.Result = NeuronDocumentProcessorResult.Error;
                    return result;
                }
                
                
                if (response.IsSuccessStatusCode)
                {
                    result.Result = NeuronDocumentProcessorResult.Success;
                }
                else
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NoContent:
                        case HttpStatusCode.NotAcceptable:
                            result.Result = NeuronDocumentProcessorResult.Fail;
                            break;
                        default:
                            result.Result = NeuronDocumentProcessorResult.Fail;
                            break;
                    }
                    result.ErrorMessage = response.ReasonPhrase;
                    result.ErrorMessage = Environment.NewLine +
                        await response.Content.ReadAsStringAsync();
                }
            }

            return result;
        }
    }
}