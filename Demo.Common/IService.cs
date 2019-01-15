using System;
using System.Collections.Generic;
using System.Text;
using AsProxy;

namespace Demo.Common
{
	[FullJsonBinding]
	public interface IService
	{
		SomeCompicatedModel GiveMeMyModel(int intVal, string stringVal, A aModel);
	}
}
