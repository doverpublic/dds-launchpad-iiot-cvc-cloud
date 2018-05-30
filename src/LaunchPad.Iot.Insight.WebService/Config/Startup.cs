// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Insight.WebService
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using global::Iot.Common;

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            ServiceEventSource.Current.Message($"Launchpad Insight Web Service  - Startup");
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            this.Configuration = builder.Build();

            string message = "Configuration=[";
            foreach ( var section in this.Configuration.GetChildren() )
            {
                ManageAppSettings.AddUpdateAppSettings(section.Key, section.Value);

                string value = section.Value;

                if( section.Key.ToLower().Contains( "password" ) )
                {
                    value = "****************"; 
                }

                message += "Key=" + section.Key + " Path=" + section.Path + " Value=" + value + " To String=" + section.ToString() + "\n";
            }

            ServiceEventSource.Current.Message($"Launchpad Insight Web Service " + message + "]" );
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ServiceEventSource.Current.Message($"Launchpad Insight Web Service  - Startup - Configure Services");
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ServiceEventSource.Current.Message($"Launchpad Insight Web Service  - Startup - Configure");
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(
                routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });
        }
    }
}
