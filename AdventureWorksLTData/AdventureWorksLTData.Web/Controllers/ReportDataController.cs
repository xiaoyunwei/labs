using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdventureWorksLTData.Web.Data;
using AdventureWorksLTData.Web.Models.AdventureWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorksLTData.Web.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/ReportData")]
    public class ReportDataController : Controller
    {
        AdventureWorksLTContext _dbContext;

        public ReportDataController(AdventureWorksLTContext adventureWorksLTContext)
        {
            _dbContext = adventureWorksLTContext;
        }

        public List<SalesOrderHeader> GetSalesOrder()
        {
            return _dbContext.SalesOrderHeader.ToList();
        }
    }
}