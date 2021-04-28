using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MonthlyNewlyIncreased.Jobs;
using static System.Console;
using static MonthlyNewlyIncreased.Constant;

namespace MonthlyNewlyIncreased
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            init();
            Configuration = configuration;
        }

        public async void init()
        {
            WriteLine($"当前基地址：{realsunBaseURL}");
            await LoginRealsunJob.start();
            //入职分配定时任务
            await EntryAssignmentJob.init();
            //季度结算定时任务
            await QuarterJob.init();
            //月度新增定时任务
            await MonthlyIncreasedJob.init();
            //剩余清零定时任务
            await AnnualLeaveResidueResetJob.init();
            //社保信息同步定时任务
            await SyncSocialSecurityMonthsJob.init();
            //年初创建定时任务
            await CreateYearBeginningAndIntoLastYearJob.init();
            //同步银行卡信息定时任务
            //await SyncBankCardJob.init();
            await LoginRealsunJob.init();
            //同步微信端年假累计申请和移动端冻结
            await SyncAnnualLeaveJob.init();
        }


        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMvc();

            services.AddMvc()
                .AddNewtonsoftJson();

            services.AddMvc(o =>
            {
                // action 过滤器
                o.Filters.Add<ActionFilter>();
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "人员信息同步EmpolyeeConnect API 文档", Version = "v1" });

                var name = typeof(Startup).Assembly.GetName() + ".xml";

                var filePath = Path.Combine(System.AppContext.BaseDirectory, "MonthlyNewlyIncreased.xml");
                c.IncludeXmlComments(filePath);
            });

            // 关闭参数自动校验,我们需要返回自定义的格式
            services.Configure<ApiBehaviorOptions>((o) => { o.SuppressModelStateInvalidFilter = true; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(options =>
            {
                options.AllowAnyHeader();
                options.AllowAnyMethod();
                options.AllowAnyOrigin();
                //options.AllowCredentials();
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseSwagger();

            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"); });
        }
    }
}