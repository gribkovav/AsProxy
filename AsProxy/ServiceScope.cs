using System;
using System.Collections.Generic;
using System.Text;

namespace AsProxy
{
	public class ServiceScope<T> : IDisposable where T : class
	{
		private readonly T _localInstance;
		private WebApiScope<T> _innerLogic;
		private readonly IServiceInstanceResolver _resolver;

		public ServiceScope(IServiceInstanceResolver resolver = null)
		{
			_resolver = resolver ?? new DefaultServiceInstanceResolver();
			if (_resolver.CanResolve<T>(out var localInstance))
			{
				_localInstance = localInstance;
			}
		}

		public TimeSpan? Timeout { get; set; }

		public T Service
		{
			get
			{
				if (_localInstance != null)
					return _localInstance;
				if (_innerLogic == null)
					_innerLogic = new WebApiScope<T>(FormFullUrl(_resolver.ServiceUrl ?? ServiceScopeUrlSettings.ServiceUrls[typeof(T).Name])
						, Encoding.UTF8
						, Timeout);
				return _innerLogic.Service;
			}
		}

		private static string FormFullUrl(string baseUrl)
		{
			var iTypeName = typeof(T).Name;
			return $"{baseUrl.TrimEnd('/', '\\')}/{iTypeName.TrimStart('I')}";
		}

		public void Dispose()
		{
		}
	}
}
