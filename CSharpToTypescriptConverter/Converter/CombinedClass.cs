using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToTypescriptConverter.Converter
{
	public class CombinedClass
	{
		public string CSharpName { get; set; }
		public List<ClassDeclarationSyntax> ClassDeclarations { get; set; }
	}
}
