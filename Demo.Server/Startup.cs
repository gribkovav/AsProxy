using System;
using System.Collections.Generic;
using System.Text;
using AsProxy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Demo.Server
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvcCore(o =>
			{
				o.ModelBinderProviders.Insert(0, new FullJsonModelBinderProvider());
				o.ValueProviderFactories.Insert(0, new FullJsonValueProviderFactory());
			})
			.AddJsonFormatters()
			.AddJsonOptions(o =>
				{
					o.SerializerSettings.TypeNameHandling = TypeNameHandling.All;
					var resolver = o.SerializerSettings.ContractResolver;
					if (resolver != null)
					{
						var res = resolver as DefaultContractResolver;
						res.NamingStrategy = null;  // remove camelcase
					}
				})
			.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole();
			loggerFactory.AddDebug();

			//app.UseMiddleware<CustomExceptionMiddleware>();
			app.UseMvcWithDefaultRoute();
		}
	}
}
