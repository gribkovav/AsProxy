using System;
using System.Collections.Generic;
using System.Text;
using AsProxy;
using Demo.Common;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Server.Controllers
{
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
}
