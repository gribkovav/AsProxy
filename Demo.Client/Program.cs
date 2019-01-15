using System;
using AsProxy;
using Demo.Common;
using Newtonsoft.Json;

namespace Demo.Client
{
	class Program
	{
		static void Main(string[] args)
		{
			ServiceScopeUrlSettings.BaseUrl = "http://localhost:8080";
			Console.WriteLine("Press Enter to make a request...");
			Console.ReadLine();
			using (ServiceScope<IService> scope = new ServiceScope<IService>())
			{
				
				var result = scope.Service.GiveMeMyModel(120, "Some string", new C());

				Console.WriteLine("Made a call to service");
				Console.WriteLine("Result:");
				Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
				Console.WriteLine("Typehandling:");
				for(int i=0;i<result.SomeArray.Length;i++)
				{
					A obj = (A) result.SomeArray[i];
					Console.WriteLine($"result.SomeArray[{i}] says :");
					obj.WhoAmI();
				}
			}

			Console.ReadLine();
		}
	}
}
