using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpToTypescriptConverter.Converter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpToTypescriptConverter.Test
{
	public static class GeneratorHelper
	{
		public static void AssertEqual(string input, string expectedOutput, Action<TypeScriptGenerator> options = null)
		{
			var sb = new StringBuilder();
			var generator = new TypeScriptGenerator(new StringWriter(sb));
			options?.Invoke(generator);
			generator.Generate(input);

			var expexted = NormalizeTypescriptSource(expectedOutput);
			var actual = NormalizeTypescriptSource(sb.ToString());

			Assert.AreEqual(expexted, actual);
		}

		private static string NormalizeTypescriptSource(string source)
		{
			return new Regex(@"\s+").Replace(source, "");
		}
	}
}
