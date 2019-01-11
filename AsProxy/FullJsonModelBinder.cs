using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using IValueProvider = Microsoft.AspNetCore.Mvc.ModelBinding.IValueProvider;

namespace AsProxy
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method)]
	public class FullJsonBindingAttribute : Attribute
	{
	}

	public class FullJsonModelBinder : IModelBinder
	{
		ConcurrentDictionary<TypeInfo, bool> _allowedTypes = new ConcurrentDictionary<TypeInfo, bool>();
		public static readonly Dictionary<Type, bool> PredefinedTypes = new Dictionary<Type, bool>() { }; //TODO: initialization

		public System.Threading.Tasks.Task BindModelAsync(ModelBindingContext bindingContext)
		{
			if (!(bindingContext.ActionContext.ActionDescriptor is ControllerActionDescriptor descriptor))
			{
				bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Expected AspNetCore.ControllerActionDescriptor as ActionDescriptor");
				return System.Threading.Tasks.Task.CompletedTask;
			}

			var shouldUseBinder = _allowedTypes.GetOrAdd(descriptor.ControllerTypeInfo, (t) =>
			{
				return t.GetCustomAttributes(typeof(FullJsonBindingAttribute), true).Any()
					|| t.GetInterfaces()
						.Any(i => i.GetCustomAttribute<FullJsonBindingAttribute>() != null || PredefinedTypes.ContainsKey(i) || i.GetCustomAttribute<JsonObjectAttribute>() != null);
			});


			if (shouldUseBinder)
			{
				if (bindingContext.ValueProvider is CompositeValueProvider composite)
				{
					if (composite.FirstOrDefault() is FullJsonValueProviderFactory.FullJsonValueProvider provider
						&& provider.TopLevelObjects.TryGetValue(bindingContext.FieldName ?? bindingContext.ModelName, out var result))
					{
						object exactType = null;
						if (result != null)
						{
							var resType = bindingContext.ModelMetadata.ModelType;
							var conversionType = resType.IsNullableType() ? Nullable.GetUnderlyingType(resType) : resType;
							exactType = ConversionParam(result, conversionType);
						}
						bindingContext.Result = ModelBindingResult.Success(exactType);
						return System.Threading.Tasks.Task.CompletedTask;
					}
				}
			}

			return null;
		}

		private object ConversionParam(object result, Type conversionType)
		{
			object exactType;
			if (conversionType.IsEnum)
			{
				exactType = Enum.Parse(conversionType, result.ToString());
			}
			else if (conversionType == typeof(Guid))
			{
				exactType = Guid.Parse(result.ToString());
			}
			else if (conversionType == typeof(TimeSpan))
			{
				exactType = TimeSpan.Parse(result.ToString(), CultureInfo.InvariantCulture);
			}
			else if (conversionType == typeof(object) || result.GetType().IsSubclassOf(conversionType))
			{
				exactType = result;
			}
			else
			{
				exactType = Convert.ChangeType(result, conversionType);
			}

			return exactType;
		}
	}

	public class FullJsonModelBinderProvider : IModelBinderProvider
	{
		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			if (context == null)
				throw new ArgumentException(nameof(context));

			return new FullJsonModelBinder();
		}
	}

	public class FullJsonValueProviderFactory : IValueProviderFactory
	{
		public System.Threading.Tasks.Task CreateValueProviderAsync(ValueProviderFactoryContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			var request = context.ActionContext.HttpContext.Request;
			if (request.ContentType == "application/fulljson")
			{
				return AddValueProviderAsync(context);
			}

			return System.Threading.Tasks.Task.CompletedTask;
		}

		private static async System.Threading.Tasks.Task AddValueProviderAsync(ValueProviderFactoryContext context)
		{
			var request = context.ActionContext.HttpContext.Request;

			string body;

			request.EnableRewind();

			using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
			{
				body = await reader.ReadToEndAsync();
			}

			request.Body.Position = 0;

			if (string.IsNullOrWhiteSpace(body))
			{
				return;
			}

			var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(body, SerializationHelper.DefaultSettingsAll);
			var valueProvider = new FullJsonValueProvider(dict, CultureInfo.CurrentCulture);

			context.ValueProviders.Add(valueProvider);
		}

		public class FullJsonValueProvider : IValueProvider
		{
			private readonly Dictionary<string, object> _dict;
			private CultureInfo _culture;

			public FullJsonValueProvider(Dictionary<string, object> dict, CultureInfo currentCulture)
			{
				_dict = dict;
				_culture = currentCulture;
			}

			public Dictionary<string, object> TopLevelObjects => _dict;

			public bool ContainsPrefix(string prefix)
			{
				return false;
			}

			public ValueProviderResult GetValue(string key)
			{
				return ValueProviderResult.None;
			}
		}
	}
}
