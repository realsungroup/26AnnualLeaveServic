using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using ShopAPI.Jobs;
using static System.Console;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Serialization;

namespace ShopAPI {
    public class Startup {
        public Startup (IConfiguration configuration) {
            init ();
            Configuration = configuration;
        }

        public async void init () {
            await LoginRealsunJob.init ();
            LoginRealsunJob.start ();

            await SyncGoodsJob.init ();

            System.Timers.Timer t = new System.Timers.Timer (10 * 1000);
            t.Elapsed += new System.Timers.ElapsedEventHandler (timeout);
            t.AutoReset = false;
            t.Enabled = true;
        }

        public static void timeout (object source, System.Timers.ElapsedEventArgs e) {
            // 上下架商品
            GroundingJob.start ();
            UndercarriageJob.start ();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            services.AddControllers ();
            services.AddMvc ();

            services.AddMvc ()
                .AddNewtonsoftJson ();

            services.AddMvc (o => {
                // action 过滤器
                o.Filters.Add<ActionFilter> ();
            });

            services.AddSwaggerGen (c => {
                c.SwaggerDoc ("v1", new OpenApiInfo { Title = "人员信息同步EmpolyeeConnect API 文档", Version = "v1" });

                var name = typeof (Startup).Assembly.GetName () + ".xml";

                var filePath = Path.Combine (System.AppContext.BaseDirectory, "ShopAPI.xml");
                c.IncludeXmlComments (filePath);
            });

            // 关闭参数自动校验,我们需要返回自定义的格式
            services.Configure<ApiBehaviorOptions> ((o) => {
                o.SuppressModelStateInvalidFilter = true;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            }

            app.UseHttpsRedirection ();

            app.UseRouting ();

            app.UseAuthorization ();

            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });

            app.UseSwagger ();

            app.UseSwaggerUI (c => {
                c.SwaggerEndpoint ("/swagger/v1/swagger.json", "v1");
            });
        }
    }
}