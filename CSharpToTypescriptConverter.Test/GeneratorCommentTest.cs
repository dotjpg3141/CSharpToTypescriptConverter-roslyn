using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpToTypescriptConverter.Test
{
	[TestClass]
	public class GeneratorCommentTest
	{
		[TestMethod]
		public void ClassCommentTest()
		{
			GeneratorHelper.AssertEqual(@"
/// <summary>
/// This a XML documentation comment.
/// </summary>
public class Foo {}
", @"
/**
* <summary>
* This a XML documentation comment.
* </summary>
*/
export class Foo {}
");
		}

		[TestMethod]
		public void PropertyCommentTest()
		{
			GeneratorHelper.AssertEqual(@"
public class Foo {
	/// <summary>
	/// This a XML documentation comment.
	/// </summary>
	public int Bar { get; }
}
", @"
export class Foo {
	/**
	* <summary>
	* This a XML documentation comment.
	* </summary>
	*/
	public Bar: number;
}
");
		}
	}
}
