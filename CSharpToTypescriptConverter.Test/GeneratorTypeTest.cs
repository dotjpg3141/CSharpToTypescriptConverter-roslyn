using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpToTypescriptConverter.Test
{
	[TestClass]
	public class GeneratorTypeTest
	{
		[TestMethod]
		public void StringTypeTest()
		{
			AssertType("string", "string");
			AssertType("String", "string");
		}

		[TestMethod]
		public void GenericTypeTest()
		{
			AssertType("MyGeneric<Foo>", "MyGeneric<Foo>");
		}

		[TestMethod]
		public void ArrayTypeTest()
		{
			AssertType("int[]", "number[]");
		}

		[TestMethod]
		public void ListTypeTest()
		{
			AssertType("List", "any[]");
		}

		[TestMethod]
		public void GenericListTypeTest()
		{
			AssertType("List<Foo>", "Foo[]");
		}

		private static void AssertType(string csharpType, string typescriptType)
		{
			GeneratorHelper.AssertEqual(@"
public class Foo {
	public " + csharpType + @" Bar { get; }
}
", @"
export class Foo
		{
			public Bar: " + typescriptType + @";
}
");
		}

	}
}
