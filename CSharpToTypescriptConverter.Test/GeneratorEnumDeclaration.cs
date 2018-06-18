using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpToTypescriptConverter.Test
{
	[TestClass]
	public class GeneratorEnumDeclaration
	{
		[TestMethod]
		public void EmptyEnumeTest()
		{
			GeneratorHelper.AssertEqual(@"
public enum Foo {}
", @"
export enum Foo {}
");
		}

		[TestMethod]
		public void EnumWithMembersWithImplicitValuesTest()
		{
			GeneratorHelper.AssertEqual(@"
public enum Foo {
	Value1, Value2
}
", @"
export enum Foo {
	Value1, Value2,
}
");
		}

		[TestMethod]
		public void EnumWithMembersWithExplicitValuesTest()
		{
			GeneratorHelper.AssertEqual(@"
public enum Foo {
	Value1 = 48,
	Value2 = 11,
	Value3,
}
", @"
export enum Foo {
	Value1 = 48,
	Value2 = 11,
	Value3,
}
");
		}
	}
}
