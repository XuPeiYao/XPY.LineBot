using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Line;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XPY.LineBot.Core;
using XPY.LineBot.Plugins.Echo;
using XPY.LineBot.Plugins.GitHubWebHook.Controllers;
using XPY.LineBot.Plugins.PttBeauty;
using XPY.LineBot.Plugins.TaiwanWeather;

namespace XPY.LineBot {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddSingleton(typeof(ILineConfiguration), (IServiceProvider serviceProvider) => {
                var instance = new LineConfiguration(); //建立Line機器人實例
                Configuration.Bind("LineConfiguration", instance); //綁定屬性
                return instance;
            });
            services.AddSingleton(typeof(ILineBot), typeof(Line.LineBot));

            PluginManager.Instance.ConfigurePlugin(
                services,
                typeof(EchoResponder).Assembly,
                typeof(PttBeautyResponder).Assembly,
                typeof(WeatherResponder).Assembly,
                typeof(CiController).Assembly
                )
                .GetAwaiter().GetResult();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.Run(PluginManager.Instance.Run);
        }
    }
}
