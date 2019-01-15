using System;
using System.Collections.Generic;
using System.Text;

namespace AsProxy
{
	public static class ServiceScopeUrlSettings
	{
		public static string BaseUrl { get; set; }

		public const string BaseUrlKey = "StartupUrl";

		public static IServiceProvider ServiceProvider { get; set; }

		public static Dictionary<string, string> ServiceUrls { get; set; }
	}

	public interface IServiceInstanceResolver
	{
		T Resolve<T>() where T : class;

		bool CanResolve<T>(out T service) where T : class;

		string ServiceUrl { get; }
	}

	public class DefaultServiceInstanceResolver : IServiceInstanceResolver
	{
		public T Resolve<T>() where T : class
		{
			return ServiceScopeUrlSettings.ServiceProvider?.GetService(typeof(T)) as T;
		}

		public bool CanResolve<T>(out T service) where T : class
		{
			service = Resolve<T>();
			return service != null;
		}

		public string ServiceUrl { get; set; } = ServiceScopeUrlSettings.BaseUrl;
	}
}
