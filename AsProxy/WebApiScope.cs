using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Castle.DynamicProxy;
using Newtonsoft.Json;
using RestSharp;
using Castle.DynamicProxy.Generators.Emitters;
using Castle.DynamicProxy.Internal;
using Microsoft.AspNetCore.Mvc;

namespace AsProxy
{
	public class WebApiScope<T> where T : class
	{
		private int? _timeout = null;

		public WebApiScope(string url,  Encoding encoding = null, TimeSpan? timeout = null)
		{
			var restClient = new RestClient(url)
			{
				Encoding = encoding ?? Encoding.UTF8,
			};
			Service = ProxyHelper.ProxyGenerator.CreateInterfaceProxyWithoutTarget<T>(new WebApiInterceptor<T>(restClient, timeout));
		}

		public T Service
		{
			get; private set;
		}

		public class WebApiInterceptor<T> : IInterceptor
		{
			private RestClient _client;
			private TimeSpan _timeout = TimeSpan.FromMinutes(10);

			public WebApiInterceptor(RestClient client, TimeSpan? timeout = null)
			{
				_client = client;
				_timeout = timeout ?? _timeout;
			}

			public void Intercept(IInvocation invocation)
			{
				var request = new RestRequest(invocation.Method.Name) { RequestFormat = DataFormat.Json, Timeout = Convert.ToInt32(_timeout.TotalMilliseconds) };

				var info = typeof(T).GetAllInterfaces().Select(t => t.GetMethod(invocation.Method.Name)).FirstOrDefault(r => r != null);
				//if (info.GetCustomAttributes(typeof(System.Web.Mvc.HttpPostAttribute), true).Any()) //TODO: we force to use POST - in this particular case its reasonable, for universal case should think about it
				{
					request.Method = Method.POST;
				}
				
				var pInfos = info.GetParameters();
				var nonPrimitiveIncluded = false;

				if (info.DeclaringType != null && (FullJsonModelBinder.PredefinedTypes.ContainsKey(info.DeclaringType) ||
					(info.DeclaringType.GetCustomAttribute<FullJsonBindingAttribute>() != null
					 || info.DeclaringType.GetCustomAttribute<JsonObjectAttribute>() != null))) //Check if interface supports dirty magic
				{
					var dict = new Dictionary<string, object>(pInfos.Length);
					for (var i = 0; i < pInfos.Length; i++)
					{
						var value = i < invocation.Arguments.Length ? invocation.Arguments[i] : null;
						var pInfo = pInfos[i];
						dict[pInfo.Name] = value;
					}
					request.AddHeader("ContentType", "application/fulljson");
					request.AddParameter("application/fulljson", JsonConvert.SerializeObject(dict, SerializationHelper.DefaultSettingsAll), ParameterType.RequestBody);

				}
				else //Fallback to usual webapi and its despare
					for (var i = 0; i < invocation.Arguments.Length; i++)
					{
						var value = invocation.Arguments[i];
						var pInfo = pInfos[i];

						if (pInfo.GetCustomAttributes(typeof(FromBodyAttribute), true).Any())
						{
							request.AddParameter("application/json",
								JsonConvert.SerializeObject(value, SerializationHelper.DefaultSettingsAll), ParameterType.RequestBody);
							nonPrimitiveIncluded = true;
						}
						else
						{
							if (IsPrimitive(pInfo.ParameterType))
							{
								request.AddParameter(pInfo.Name, value);
							}
							else
							{
								if (nonPrimitiveIncluded)
									throw new InvalidOperationException("More than one primitive parameter in method signature");

								request.AddParameter("application/json",
									JsonConvert.SerializeObject(value, SerializationHelper.DefaultSettingsAll), ParameterType.RequestBody);
								nonPrimitiveIncluded = true;
							}
						}
					}
				var response = _client.Execute(request);
				if (response.IsSuccessful)
				{
					if (info.ReturnType != typeof(void))
					{
						invocation.ReturnValue = JsonConvert.DeserializeObject(response.Content, info.ReturnType, SerializationHelper.DefaultSettingsAll);
					}
					return;
				}
				if (!string.IsNullOrWhiteSpace(response.Content) && ErrorDetails.TryParseJson(response.Content, out var details))
				{
					if (details.Exception != null)
						throw new Exception(details.Exception.Message, details.Exception);
					throw new InvalidOperationException(details.Message);
				}
				if (response.ErrorException != null)
					throw response.ErrorException;
				throw new InvalidOperationException(JsonConvert.SerializeObject(response));
			}

			public static bool IsOK(IRestResponse response)
			{
				return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;
			}

			public static bool IsPrimitive(Type type)
			{
				return type.IsPrimitive
					   || type.IsNullableType()
					   || type == typeof(String)
					   || type == typeof(Decimal)
					   || type == typeof(DateTime)
					   || type == typeof(TimeSpan)
					   || type == typeof(DateTimeOffset)
					   || type == typeof(Guid)
					   || type.IsEnum
					   || (type.IsArray && IsPrimitive(type.GetElementType()));
			}
		}
	}

	public static class ProxyHelper
	{
		private static readonly Lazy<ProxyGenerator> _proxyGenerator = new Lazy<ProxyGenerator>(LazyThreadSafetyMode.ExecutionAndPublication);

		public static ProxyGenerator ProxyGenerator => _proxyGenerator.Value;
	}
}
