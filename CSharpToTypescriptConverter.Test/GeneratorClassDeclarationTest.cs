using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpToTypescriptConverter.Test
{
	[TestClass]
	public class GeneratorClassDeclarationTest
	{
		[TestMethod]
		public void SimpleClassTest()
		{
			GeneratorHelper.AssertEqual(@"
public class Foo {}
", @"
export class Foo {}
");
		}

		[TestMethod]
		public void NonPublicClassTest()
		{
			GeneratorHelper.AssertEqual(@"
class Foo {}
", @"
");
		}

		[TestMethod]
		public void ClassWithNamespaceTest()
		{
			GeneratorHelper.AssertEqual(@"
namespace Baz {
	public class Foo {}
}
", @"
export class Foo {}
");
		}

		[TestMethod]
		public void PartialClassTest()
		{
			GeneratorHelper.AssertEqual(@"
public partial class Foo {
	public string Prop1 { get; }
}

public partial class Foo {
	public string Prop2 { get; }
}
", @"
export class Foo {
	public Prop1: string;
	public Prop2: string;
}
");
		}

	}
}
