using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpToTypescriptConverter.Converter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpToTypescriptConverter.Test
{
	[TestClass]
	public class GeneratorErrorTest
	{
		[TestMethod]
		[ExpectedException(typeof(SyntaxErrorException))]
		public void SyntaxErrorTest()
		{
			GeneratorHelper.AssertEqual("class Foo {", "");
		}
	}
}
