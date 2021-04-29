using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Banter.Reports.BLL.Interface
{
    public interface IReport_Service
    {
        Task<JObject> GetGroupDetails(string other_id);
    }
}
