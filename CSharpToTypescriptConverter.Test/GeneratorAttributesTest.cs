using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpToTypescriptConverter.Test
{
	[TestClass]
	public class GeneratorAttributesTest
	{
		[TestMethod]
		public void ClassAttributeTest()
		{
			GeneratorHelper.AssertEqual(@"
[MyAttribute()]
public class Foo {}
", @"
// MyAttribute()
export class Foo {}
");
		}

		[TestMethod]
		public void PropertyAttributeTest()
		{
			GeneratorHelper.AssertEqual(@"
public class Foo {
	[MyAttribute()]
	public int Bar { get; }
}
", @"
export class Foo {
	// MyAttribute()
	public Bar: number;
}
");
		}
	}
}
