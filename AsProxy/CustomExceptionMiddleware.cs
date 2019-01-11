using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace AsProxy
{
	public class CustomExceptionMiddleware
	{
		private readonly RequestDelegate next;

		public CustomExceptionMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public async System.Threading.Tasks.Task InvokeAsync(HttpContext context)
		{
			try
			{
				await next.Invoke(context);
			}
			catch (Exception exc)
			{
				//TODO: Log with standart logging pipeline
				//_log.Error(exc.Message, exc);
				await HandleExceptionAsync(context, exc);
			}
		}

		private System.Threading.Tasks.Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			var result = new ErrorDetails()
			{
				Message = exception.Message,
				StatusCode = (int)HttpStatusCode.InternalServerError,
				Exception = exception,
				ExceptionText = exception.ToString()
			};
			context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
			return context.Response.WriteAsync(result.ToJson());
		}
	}

	public class ErrorDetails
	{
		public int StatusCode { get; set; }
		public string Message { get; set; }
		public string ExceptionText { get; set; }
		public Exception Exception { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, SerializationHelper.DefaultSettingsAll);
		}

		public static bool TryParseJson(string jsonString, out ErrorDetails details)
		{
			try
			{
				details = JsonConvert.DeserializeObject<ErrorDetails>(jsonString, SerializationHelper.DefaultSettingsAll);
				return true;
			}
			catch
			{
				details = null;
				return false;
			}
		}
	}
}
