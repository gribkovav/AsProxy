using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.Common
{
	public class SomeCompicatedModel
	{
		public int SomeInt { get; set; }

		public object[] SomeArray { get; set; }

		public string SomeString { get; set; }
	}

	public abstract class A
	{
		public virtual void WhoAmI()
		{
			Console.WriteLine("I'm A-class");
		}
	}

	public  class B : A
	{
		public override void WhoAmI()
		{
			Console.WriteLine("I'm D-class");
		}
	}

	public  class C : A
	{
		public override void WhoAmI()
		{
			Console.WriteLine("I'm C-class");
		}
	}
}
