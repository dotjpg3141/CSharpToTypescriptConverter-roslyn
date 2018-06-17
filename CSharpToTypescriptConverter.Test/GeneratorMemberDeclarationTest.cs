using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpToTypescriptConverter.Test
{
	[TestClass]
	public class GeneratorMemberDeclarationTest
	{
		[TestMethod]
		public void PropertyWithGetterAndSetterTest()
		{
			GeneratorHelper.AssertEqual(@"
public class Foo {
	public MyType MyProperty { get; set; }
}
", @"
export class Foo {
	public MyProperty: MyType;
}
");
		}

		[TestMethod]
		public void PropertyWithGetterTest()
		{
			GeneratorHelper.AssertEqual(@"
public class Foo {
	public MyType MyProperty { get; }
}
", @"
export class Foo {
	public MyProperty: MyType;
}
");
		}

		[TestMethod]
		public void PropertyWithSetterTest()
		{
			GeneratorHelper.AssertEqual(@"
public class Foo {
	public MyType MyProperty { set { } }
}
", @"
export class Foo {
}
");
		}

		[TestMethod]
		public void PropertyWithPrivateGetterTest()
		{
			GeneratorHelper.AssertEqual(@"
public class Foo {
	public MyType MyProperty { private get; }
}
", @"
export class Foo {
}
");
		}

		[TestMethod]
		public void StaticPropertyTest()
		{
			GeneratorHelper.AssertEqual(@"
public class Foo {
	public static MyType MyProperty { get; }
}
", @"
export class Foo {
}
");
		}
	}
}
