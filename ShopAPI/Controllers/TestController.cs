using System;
using System.Threading.Tasks;
using FastJSON;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
            var getNewEmployee = new NewEmployee();
             await  getNewEmployee.GetNewEmployeeList();
             Console.WriteLine(JsonConvert.SerializeObject(getNewEmployee.employeeList));
             foreach (var item in getNewEmployee.employeeList)
             {
                await getNewEmployee.Distribution(item.jobId);
             }
            return Ok(new {});
        }
        
    }
}