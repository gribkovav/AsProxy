
[![Build Status](https://travis-ci.org/gribkovav/AsProxy.svg?branch=master)](https://travis-ci.org/gribkovav)

# AsProxy - What is it?
 You can call it a library (though not published as nuget-packet, needs some polish and fine tuning) or a proof of concept, call it the way you like. All you need to try it is to clone and open **asproxy.sln** in Visual Studio (VS 2017 preferably). Then run Demo.Server and Demo.Client and you've got the idea. 
 
Ok, but what do I need it for you ask.  Sometimes you have full control over both server and client codebase, both are written in C# and do some interaction over http (asp core). Yes you do have things like swagger, but it could be a little too compex (IMHO) and sometimes is overkill. There are times I just like to use old WCF style with `Channel<T>` but without all headache with WCF-configuration. Here you have a working solution which I wrote in need of migrating from WCF to Kestrel server. I stripped few things about authentication and authorization, simplified things a little (I've already mentioned you do need some finetuning for this).

# How to use?

Put AsProxy in dependencies.
Create future service interface in common lib.
For example:
``` csharp
[FullJsonBinding]
public interface IService
{
	SomeCompicatedModel GiveMeMyModel(int intVal, string stringVal, A aModel);
}
```
`FullJsonBindingAttribute` is quite important here (for now lets just remember it).

In our server part we do following in `Startup.cs` (or any other class where you do all config for hosting environment):
```csharp
public void ConfigureServices(IServiceCollection services)
{
	...
	
	services.AddMvcCore(o => //You can use AddMvc here without any problem, its me who went lightweight
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
	...
```
`FullJsonModelBinderProvider` and `FullJsonModelBinderProvider` do all the magic on serverside.
Then we create controller:
``` csharp
public class ServiceController : Microsoft.AspNetCore.Mvc.ControllerBase, IService
{
	public SomeCompicatedModel GiveMeMyModel(int intVal, string stringVal, A aModel)
	{
		SomeCompicatedModel result = new SomeCompicatedModel()
		{
			SomeArray = new object[]{aModel, new B(), new C()},
			SomeInt = intVal,
			SomeString = stringVal
		};

		return result;
	}

	[HttpGet]
	public  ActionResult Ping()
	{
		return Ok("I'm a pretty service");
	}
}
```
Mind the naming convention: `IService` goes to `ServiceController`. Yes, it makes some kind of restriction - but it's easy to surpass with little efforts (if you show some kind of interest I'll gladly write an update). 
BTW: `Ping` method is not part of `IService` - you are free in usage of your controllers aside of interface implementation.

And on client we do:
``` csharp
ServiceScopeUrlSettings.BaseUrl = "http://localhost:8080"; //Not very convinient, I know I know, you can make it better - send pull requests :)
Console.WriteLine("Press Enter to make a request...");
Console.ReadLine();
using (ServiceScope<IService> scope = new ServiceScope<IService>()) //In form of IDisposable just to better see scope
{
	var result = scope.Service.GiveMeMyModel(120, "Some string", new C()); //

	Console.WriteLine("Made a call to service");
	Console.WriteLine("Result:");
	Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
	Console.WriteLine("Typehandling:");
	for(int i=0;i<result.SomeArray.Length;i++)
	{
		A obj = (A) result.SomeArray[i];
		Console.WriteLine($"result.SomeArray[{i}] says :");
		obj.WhoAmI(); //See implementation details of A, B, C, D
	}
}
```

And I highly recommend to use `CustomExceptionMiddleware` to use `try-catch` block as if you do it on server side.
**That's all folks!**
