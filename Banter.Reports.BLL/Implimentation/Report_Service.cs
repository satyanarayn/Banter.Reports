using Banter.Reports.BLL.Interface;
using Banter.Reports.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Banter.Reports.BLL.Implimentation
{
    public class Report_Service:IReport_Service
    {
        private IConfiguration _configuration;
        private ILogger<Report_Service> _logger;
        private ISchedulerHttpClient _schedulerHttpClient;
        public Report_Service( IConfiguration configuration, ILogger<Report_Service> logger, ISchedulerHttpClient schedulerHttpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _schedulerHttpClient = schedulerHttpClient;
        }
        public async Task<JObject> GetGroupDetails(string other_id)
        {
            string base_url = _configuration.GetSection("SC:base_url").Value;
            string resource_url = _configuration.GetSection("SC:recent_thread").Value;
            string url = base_url + resource_url;
            var payload = JObject.FromObject(new
            {
                select_columns = new[] { "id", "other_id", "other_name", "message", "status", "msg_time", "temp_id" },
                filter_groups = JObject.FromObject(new
                {
                    filters = new[]
                    {
                        JObject.FromObject(new{field= "other_type",cond="eq",val="group"}),
                        JObject.FromObject(new{field= "other_id",cond="eq",val=other_id}),
                        JObject.FromObject(new{field= "status",cond="eq",val="delivery success"}),
                    }
                })
            });
            payload["filter_groups"]["operator"] = "and";
            string access_token = await GetAnonamousToken();
            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                { "access_key",access_token},
                { "application_id",_configuration.GetSection("SC:application_id").Value}
            };
           var response= await _schedulerHttpClient.PostAsync<JObject>(url, payload, false,false,dict);
            return response;
        }
        public async Task<string> GetAnonamousToken()
        {
            string base_url = _configuration.GetSection("SC:base_url").Value;
            string resource_url = _configuration.GetSection("SC:AnonymousUser:TokenUrl").Value;
            string url = base_url + resource_url;
            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
            {"client_id",_configuration.GetSection("SC:client_id").Value},
            {"client_secret",_configuration.GetSection("SC:client_secret").Value},
            };
            var response = await _schedulerHttpClient.PostFormUrlAsync<JObject>(url, dict, false);
            return response["access_token"].ToString();
        }

    }
}
