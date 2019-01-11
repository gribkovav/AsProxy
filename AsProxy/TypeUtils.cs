using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AsProxy
{
	static class TypeUtils
	{
		public static bool IsNullableType(this Type type) => type.GetTypeInfo().IsGenericType &&  type.GetGenericTypeDefinition() == typeof(Nullable<>);
	}
}
