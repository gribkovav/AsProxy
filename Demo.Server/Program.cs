using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Demo.Server
{
	class Program
	{
		private const string HostUrl = "http://*:8080";
		static void Main(string[] args)
		{
			//var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			//var builder = new ConfigurationBuilder()
			//	.SetBasePath(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName))
			//	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
			//	.AddJsonFile($"appsettings.{environment}.json", optional: true);

			var host = new WebHostBuilder()
				.UseKestrel()
				.UseUrls(HostUrl)
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup<Startup>()
				.Build();
			host.Start();
			Console.WriteLine($"Started listening at {HostUrl}");
			Console.ReadLine();
		}
	}
}
