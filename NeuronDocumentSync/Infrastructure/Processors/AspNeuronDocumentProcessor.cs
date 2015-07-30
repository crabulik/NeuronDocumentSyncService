using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading.Tasks;
using NeuronDocumentSync.Enums;
using NeuronDocumentSync.Interfaces;
using NeuronDocumentSync.Models;
using NeuronDocumentSync.Resources;
using Newtonsoft.Json;

namespace NeuronDocumentSync.Infrastructure.Processors
{

    internal class WebApiSendResult
    {
        public NeuronDocumentProcessorResult Result;

        public string ErrorMessage;
    }

    public class AspNeuronDocumentProcessor : INeuronDocumentProcessor
    {
        private const string BearerValue = "Bearer";
        private const string PublishUrl = "api/documentpublisher";
        private const string ImportUrl = "api/import";
        private const string TokenUrl = "Token";
        private const string GrantTypeParamName = "grant_type";
        private const string GrantTypeParamValue = "password";
        private const string UserNameParamName = "username";
        private const string PasswordParamName = "Password";
        private const string AccessTokenParamName = "access_token";
        private const string CacheTokenName = "AspNeuronDocumentProcessorToken";
        private const double CachedTokenLifeTimeMinutes = 300;
        


        private readonly DocumentConverter _converter;
        private readonly IGeneralConfig _cfg;
        private readonly INeuronLogger _logger;
        private MemoryCache Cache { get; set; }

        public AspNeuronDocumentProcessor(DocumentConverter converter, IGeneralConfig cfg, INeuronLogger logger)
        {
            _converter = converter;
            _cfg = cfg;
            _logger = logger;
            Cache = MemoryCache.Default;
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
            var token = GetToken();
            if (token == null)
            {
                result.Result = NeuronDocumentProcessorResult.PassOrLoginError;
                return result;
            }
            using (var client = CreateClient(token))
            {
                client.BaseAddress = new Uri(_cfg.WebImportUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response;
                try
                {
                    response = await client.GetAsync(PublishUrl);
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
                    //response.Content.Headers.ContentType
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
            var token = GetToken();
            if (token == null)
            {
                result.Result = NeuronDocumentProcessorResult.PassOrLoginError;
                return result;
            }
            using (var client = CreateClient(token))
            {
                client.BaseAddress = new Uri(_cfg.WebImportUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response;
                try
                {
                    response = await client.PostAsJsonAsync(ImportUrl, document);
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

        private Dictionary<string, string> GetTokenDictionary(string userName, string password)
        {
            var pairs = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>( GrantTypeParamName, GrantTypeParamValue ), 
                    new KeyValuePair<string, string>( UserNameParamName, userName ), 
                    new KeyValuePair<string, string> ( PasswordParamName, password )
                };
            var content = new FormUrlEncodedContent(pairs);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_cfg.WebImportUrl);
                var response =
                    client.PostAsync(TokenUrl, content).Result;
                if(!response.IsSuccessStatusCode)
                    return null;
                var result = response.Content.ReadAsStringAsync().Result;
                // Десериализация полученного JSON-объекта
                Dictionary<string, string> tokenDictionary =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                return tokenDictionary;
            }
        }

        private string GetToken()
        {
            string accessToken;
            var cached = Cache[CacheTokenName];
            if (cached != null)
            {
                accessToken = ((string)cached);
            }
            else
            {
                var tokenDictionary = GetTokenDictionary(_cfg.WebImportLogin, _cfg.WebImportPass);
                if (tokenDictionary == null)
                    return null;
                accessToken = tokenDictionary[AccessTokenParamName];
                Cache.Set(CacheTokenName, accessToken,
                    DateTimeOffset.Now.AddMinutes(CachedTokenLifeTimeMinutes));
            }

            return accessToken;
        }

        // создаем http-клиента с токеном 
        private HttpClient CreateClient(string accessToken = "")
        {
            var client = new HttpClient();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(BearerValue, accessToken);
            }
            return client;
        }
    }
}