using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AsProxy
{
	public static class SerializationHelper
	{

		public static readonly JsonSerializerSettings DefaultSettingsAll = new JsonSerializerSettings()
		{
			TypeNameHandling = TypeNameHandling.All,
			ObjectCreationHandling = ObjectCreationHandling.Replace
		};
	}
}
