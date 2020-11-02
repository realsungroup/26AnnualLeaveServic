using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ShopAPI {
    public class ActionFilter : IActionFilter {
        /// <summary>
        /// action执行前
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting (ActionExecutingContext context) {
            //校验参数
            if (!context.ModelState.IsValid) {
                var errorMsg = context.ModelState.Values.SelectMany (e => e.Errors).Select (e => e.ErrorMessage).FirstOrDefault ();
                context.Result = new OkObjectResult (new {
                    error = 1,
                        message = string.IsNullOrWhiteSpace (errorMsg) ? "参数校验错误" : errorMsg,
                        data = new { }
                });
                return;
            }
        }

        /// <summary>
        /// action执行后
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted (ActionExecutedContext context) { }
    }
}