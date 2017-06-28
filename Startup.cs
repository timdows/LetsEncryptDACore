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
					const string acmeDirectory = "/var/www/html/.well-known/acme-challenge";

					if (pathValue.StartsWith(prefixValue))
					{
						if (int.TryParse(pathValue.Replace(prefixValue, string.Empty), out var unixTimestamp))
						{
							var fileNames = Directory.GetFiles(acmeDirectory);
							foreach (var fileName in fileNames)
							{
								var filePath = Path.Combine(acmeDirectory, fileName);
								var fileInfo = new FileInfo(filePath);
								var fileDateTimeOffset = new DateTimeOffset(fileInfo.LastWriteTime);
								var fileEpoch = fileDateTimeOffset.ToUnixTimeSeconds();

								// Check for file creaded around 3 seconds of the requested
								if (Math.Abs(unixTimestamp - fileEpoch) < 3)
								{
									//await context.Response.WriteAsync($"{fileName} The match!\n");
									var fileContents = File.ReadAllText(filePath);

									await context.Response.WriteAsync(fileContents);
								}
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
