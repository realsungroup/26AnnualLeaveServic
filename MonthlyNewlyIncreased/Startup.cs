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
            WriteLine($"come on");
           await LoginRealsunJob.start();
           //await EntryAssignmentJob.init();
           //await QuarterJob.init();
           // await MonthlyIncreasedJob.init();
            //await AnnualLeaveResidueResetJob.init();
           /*
            // 定时任务
            //月度新增
            await MonthlyIncreasedJob.init();
            //同步社保信息
            await SyncSocialSecurityMonthsJob.init();
            await LoginRealsunJob.init();
          */
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
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "人员信息同步EmpolyeeConnect API 文档", Version = "v1"});

                var name = typeof(Startup).Assembly.GetName() + ".xml";

                var filePath = Path.Combine(System.AppContext.BaseDirectory, "ShopAPI.xml");
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