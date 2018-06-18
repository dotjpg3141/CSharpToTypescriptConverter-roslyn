using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpToTypescriptConverter.Test
{
	[TestClass]
	public class GeneratorOptionsTest
	{
		[TestMethod]
		public void PropertyToCamelCaseTest()
		{
			GeneratorHelper.AssertEqual(@"
public class Foo {
	public MyType MyProperty { get; set; }
}
", @"
export class Foo {
	public myProperty: MyType;
}
", options => options.ConvertFieldsToCamelCase = true);
		}

		[TestMethod]
		public void ClassToInterfaceTest()
		{
			GeneratorHelper.AssertEqual(@"
public class Foo {}
", @"
export interface Foo {}
", options => options.ConvertClassesToInterfaces = true);
		}

		[TestMethod]
		public void CustomTypePrefixTest()
		{
			GeneratorHelper.AssertEqual(@"
public class Foo {
	public Foo Property1 { get; }
	public Bar Property2 { get; }
}
", @"
export class IFoo {
	public Property1: IFoo;
	public Property2: Bar;
}
", options => options.CustomTypePrefix = "I");
		}

		[TestMethod]
		public void CustomTypePrefixWithPartialClassTest()
		{
			GeneratorHelper.AssertEqual(@"
public partial class Foo {
	public Foo Property1 { get; }
}

public partial class Foo {
	public Foo Property2 { get; }
}
", @"
export class IFoo {
	public Property1: IFoo;
	public Property2: IFoo;
}
", options => options.CustomTypePrefix = "I");
		}

		[TestMethod]
		public void EmptyEnumeTest()
		{
			GeneratorHelper.AssertEqual(@"
public enum Foo {}
", @"
export const enum Foo {}
", options => options.MakeEnumConst = true);
		}

	}
}
