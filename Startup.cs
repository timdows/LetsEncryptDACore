using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace LetsEncryptDACore
{
	public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

			//app.UseStaticFiles();

			app.Map(new PathString("/.well-known/acme-challenge"), branch =>
			{
				branch.Run(async context => {
					var pathValue = context.Request.Path.Value;
					const string prefixValue = "/letsencrypt_";

					if (pathValue.StartsWith(prefixValue))
					{
						if (int.TryParse(pathValue.Replace(prefixValue, string.Empty), out var unixTimestamp))
						{
							//await context.Response.WriteAsync(unixTimestamp);
							var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
							var dateTime = dateTimeOffset.UtcDateTime;

							await context.Response.WriteAsync(dateTime.ToString());

							var files = Directory.GetFiles("/var/www/html/.well-known/acme-challenge");
							foreach (var file in files)
							{
								await context.Response.WriteAsync(file);
							}
						}
					}
				});
			});


			app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
