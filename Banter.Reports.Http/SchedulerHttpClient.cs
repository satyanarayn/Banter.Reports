using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Banter.Reports.Http
{
    public class SchedulerHttpClient : ISchedulerHttpClient
    {
        private IHttpContextAccessor _context;
        private IConfiguration _configuration;
        private ILogger<SchedulerHttpClient> _logger;
        public SchedulerHttpClient(IHttpContextAccessor context, IConfiguration configuration, ILogger<SchedulerHttpClient> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<TResult> GetAsync<TResult>(string resourceUri, bool isScocuDefaultHeaders,bool isCalendlyHeaders=false, Dictionary<string, string> customHeaders = null)
        {
            _logger.LogInformation($"resource Url : {resourceUri}");
            TResult result = default(TResult);
            using (HttpClient client = new HttpClient())
            {
               await SetHttpClient(client, "", isScocuDefaultHeaders,isCalendlyHeaders, customHeaders);
                var response = await client.GetAsync(resourceUri).ConfigureAwait(false);
                await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
                {
                    if (x.IsFaulted)
                        throw x.Exception;
                    string responseString = x.Result;
                    result = JsonConvert.DeserializeObject<TResult>(responseString);
                    _logger.LogInformation($"client response : {JsonConvert.SerializeObject(result)}");
                });
            }
            return result;
        }

        public async Task<TResult> PostAsync<TResult>(string resourceUri, object content, bool isScocuDefaultHeaders, bool isCalendlyHeaders = false, Dictionary<string, string> customHeaders = null, string contentType = "application/json")
        {
            _logger.LogInformation($"resource Url : {resourceUri}");
            _logger.LogInformation($"client resource payload : {JsonConvert.SerializeObject(content)}");
            TResult result = default(TResult);
            using (HttpClient client = new HttpClient())
            {
               await SetHttpClient(client, contentType, isScocuDefaultHeaders, isCalendlyHeaders,customHeaders);
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(resourceUri, stringContent).ConfigureAwait(false);
                await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
                {
                    if (x.IsFaulted)
                        throw x.Exception;
                    string responseString = x.Result;
                    result = JsonConvert.DeserializeObject<TResult>(responseString);
                    _logger.LogInformation($"client response : {result}");
                });
            }
            return result;
        }

        public async Task<TResult> PutAsync<TResult>(string resourceUri, object content, bool isScocuDefaultHeaders, bool isCalendlyHeaders = false, Dictionary<string, string> customHeaders = null, string contentType = "application/json")
        {
            _logger.LogInformation($"resource Url : {resourceUri}");
            _logger.LogInformation($"client resource payload : {JsonConvert.SerializeObject(content)}");
            TResult result = default(TResult);
            using (HttpClient client = new HttpClient())
            {
                await SetHttpClient(client, contentType, isScocuDefaultHeaders, isCalendlyHeaders, customHeaders);
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
                var response = await client.PutAsync(resourceUri, stringContent).ConfigureAwait(false);
                await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
                {
                    if (x.IsFaulted)
                        throw x.Exception;
                    string responseString = x.Result;
                    result = JsonConvert.DeserializeObject<TResult>(responseString);
                    _logger.LogInformation($"client response : {result}");
                });
            }
            return result;
        }

        public async Task<TResult> PostFormUrlAsync<TResult>(string resourceUri, Dictionary<string, string> formUrlContentDict, bool isScocuDefaultHeaders, bool isCalendlyHeaders = false, Dictionary<string, string> customHeaders = null)
        {
            _logger.LogInformation($"resource Url : {resourceUri}");
            _logger.LogInformation($"client request payload : {JObject.FromObject(formUrlContentDict)}");
            TResult result = default(TResult);
            using (HttpClient client = new HttpClient())
            {
               await SetHttpClient(client, "application/x-www-form-urlencoded", isScocuDefaultHeaders,isCalendlyHeaders, customHeaders);
                FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(formUrlContentDict);
                var response = await client.PostAsync(resourceUri, formUrlEncodedContent).ConfigureAwait(false);
                await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
                {
                    if (x.IsFaulted)
                        throw x.Exception;
                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = x.Result;
                        result = JsonConvert.DeserializeObject<TResult>(responseString);
                        _logger.LogInformation($"client response : {JsonConvert.SerializeObject(result)}");
                    }
                    else
                    {
                        _logger.LogInformation($"client error response :{x.Result}");
                        string responseString = x.Result;
                        result = JsonConvert.DeserializeObject<TResult>(responseString);
                    }
                });
            }
            return result;
        }


        private async Task SetHttpClient(HttpClient client, string contentType, bool isScocuDefaultHeaders = false,bool isCalendlyHeaders=false, Dictionary<string, string> customHeaders = null)
        {
            client.DefaultRequestHeaders.Clear();
            if (customHeaders != null)
            {
                foreach (KeyValuePair<string, string> header in customHeaders)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            if (isScocuDefaultHeaders)
            {
                if (!string.IsNullOrEmpty(contentType))
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
                var requestHeaders = GetAllHeaders();
                foreach (var header in requestHeaders)
                {
                    if (!client.DefaultRequestHeaders.Contains(header.Key))
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            else if(isCalendlyHeaders)
            {
                string calendlyKey = await GetCalendlyToken();
                client.DefaultRequestHeaders.Add("X-TOKEN", calendlyKey);

            }
        }

        public async Task<string> GetCalendlyToken()
        {
            string stripeToken = string.Empty;
            string url = _configuration.GetSection("SC:CalendlyConnectionSerach")?.Value;

            JObject serachObject = JObject.FromObject(new
            {
                select_columns = new[] { "su_id", "ims_auth_type_id", "meta_data", "ims_connector_id" },
                filter_groups = JObject.FromObject(new
                {
                    filters = new[]
                      {
                        JObject.FromObject(new
                        {
                            field = "ims_auth_type_id",
                            cond ="eq",
                            val =_configuration.GetSection("CalendlyConnection:ims_auth_type_id")?.Value
                        }),
                        JObject.FromObject(new
                        {
                            field = "ims_connector_id",
                            cond ="eq",
                            val =_configuration.GetSection("CalendlyConnection:ims_connector_id")?.Value
                        }),
                        JObject.FromObject(new
                        {
                            field = "application_id",
                            cond ="eq",
                            val = GetValueFromHeaders("application_id")
                        })
                    }
                })
            });
            serachObject["filter_groups"]["operator"] = "and";

            var response = await PostAsync<JObject>(url, serachObject, true);
            if (response["status"].ToString() == "success" && response["data"].HasValues)
            {
                stripeToken = response["data"][0]["meta_data"]["value"].ToString();
                return stripeToken;
            }
            else
                throw new Exception("Issue while setting calendly key. please contact admin");

        }

        public Dictionary<string, string> GetAllHeaders()
        {
            List<string> unUsedHeaders = new List<string>();
            unUsedHeaders.Add("Cache-Control"); unUsedHeaders.Add("Connection"); unUsedHeaders.Add("Accept-Encoding"); unUsedHeaders.Add("Host");
            unUsedHeaders.Add("User-Agent"); unUsedHeaders.Add("Postman-Token"); unUsedHeaders.Add("Content-Type");
            unUsedHeaders.Add("Content-Length"); unUsedHeaders.Add("X-Original-Host"); unUsedHeaders.Add("X-Forwarded-Proto"); unUsedHeaders.Add("X-Forwarded-For"); unUsedHeaders.Add("Sec-Fetch-Site"); unUsedHeaders.Add("Sec-Fetch-Mode"); unUsedHeaders.Add("Sec-Fetch-Dest");

            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (_context.HttpContext != null)
            {
                foreach (var header in _context.HttpContext.Request.Headers)
                {
                    var unUsedHeaderCount = unUsedHeaders.Where(x => x.ToLower() == header.Key.ToLower()).ToList();
                    if (unUsedHeaderCount.Count == 0)
                        headers.Add(header.Key, header.Value);
                }
            }
            return headers;
        }

        public string GetValueFromHeaders(string headerName)
        {
            var headers = _context.HttpContext.Request.Headers;
            if (headers.ContainsKey(headerName))
                return headers[headerName].FirstOrDefault();
            else
                return string.Empty;
        }
    }
}
