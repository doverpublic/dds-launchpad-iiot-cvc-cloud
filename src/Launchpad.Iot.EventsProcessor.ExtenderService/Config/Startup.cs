using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using global::Iot.Common;

namespace Launchpad.Iot.EventsProcessor.ExtenderService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            ServiceEventSource.Current.Message($"Launchpad Events Processor Extender Service  - Startup");
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            this.Configuration = builder.Build();


            string message = "Configuration=[";

            foreach (var section in this.Configuration.GetChildren())
            {
                ManageAppSettings.AddUpdateAppSettings(section.Key, section.Value);

                string value = section.Value;

                if (section.Key.ToLower().Contains("password"))
                {
                    value = "****************";
                }

                message += "Key=" + section.Key + " Path=" + section.Path + " Value=" + value + " To String=" + section.ToString() + "\n";
            }
            ServiceEventSource.Current.Message("On Launchpad Events Processor Extender Sevice " + message + "]");

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ServiceEventSource.Current.Message($"Launchpad Events Processor Extender Service  - Startup - Configure Services");
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ServiceEventSource.Current.Message($"Launchpad Events Processor Extender Service  - Startup - Configure Services");
 
            app.UseMvc();
        }
    }
}
