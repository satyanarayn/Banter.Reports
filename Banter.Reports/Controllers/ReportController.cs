using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banter.Reports.BLL.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Banter.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        public readonly IReport_Service _ireportService;
        public ReportController(IReport_Service ireportService)
        {
            _ireportService = ireportService;
        }
        [HttpGet("group_details")]
        public async Task<ActionResult> Get_Group_Details(string other_id)
        {
            var response = await _ireportService.GetGroupDetails(other_id);
            return Ok(response);
        }
    }
}