using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.Common
{
	interface IServiceInterface
	{
		SomeCompicatedModel GiveMeMyModel(int intVal, string stringVal, A aModel);
	}
}
