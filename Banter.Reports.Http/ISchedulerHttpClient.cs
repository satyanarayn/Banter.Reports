using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Banter.Reports.Http
{
    public interface ISchedulerHttpClient
    {
        Dictionary<string, string> GetAllHeaders();
        string GetValueFromHeaders(string headerName);
        Task<TResult> GetAsync<TResult>(string resourceUri, bool isScocuDefaultHeaders, bool isCalendlyHeaders = false, Dictionary<string, string> customHeaders = null);
        Task<TResult> PostAsync<TResult>(string resourceUri, object content, bool isScocuDefaultHeaders, bool isCalendlyHeaders = false, Dictionary<string, string> customHeaders = null, string contentType = "application/json");
        Task<TResult> PutAsync<TResult>(string resourceUri, object content, bool isScocuDefaultHeaders, bool isCalendlyHeaders = false, Dictionary<string, string> customHeaders = null, string contentType = "application/json");
        Task<TResult> PostFormUrlAsync<TResult>(string resourceUri, Dictionary<string, string> formUrlContentDict, bool isScocuDefaultHeaders, bool isCalendlyHeaders = false, Dictionary<string, string> customHeaders = null);        
    }
}
