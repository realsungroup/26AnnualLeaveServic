using System;
using System.Threading.Tasks;
using FastJSON;
using Microsoft.AspNetCore.Mvc;
using ShopAPI.Jobs;
using ShopAPI.Tasks;

namespace ShopAPI.Controllers
{
    /// <summary>
    /// 获取 realsun 平台的 accessToken
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<OkObjectResult> Test()
        {
            var getNewEmployee = new GetNewEmployee();
            getNewEmployee.getConversionDays(4, "2021-01-04");

            // await  getNewEmployee.GetNewEmployeeList();
            // foreach (var item in getNewEmployee.employeeList)
            // {
            //    await getNewEmployee.Distribution(item.jobId);
            // }
            return Ok(new {});
        }
        
    }
}