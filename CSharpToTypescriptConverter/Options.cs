using System.Collections.Generic;
using CommandLine;

namespace CSharpToTypescriptConverter
{
	public class Options
	{
		[Option('i', "input", HelpText = "Input files to be processed.")]
		public IEnumerable<string> InputFiles { get; set; }

		[Option('d', "input-dir", HelpText = "Input directories. Searches for files with .cs ending.")]
		public IEnumerable<string> InputDirectories { get; set; }

		[Option('r', "recursice", HelpText = "Search input directories recursively.")]
		public bool RecursiveInputDirectories { get; set; }

		[Option('o', "output", HelpText = "Output file. Default is stdout")]
		public string OutputDirectory { get; set; }

		[Option('v', "verbose", HelpText = "Verbose output")]
		public bool Verbose { get; set; }

		[Option("camel-case", HelpText = "Make Typescript fields camel case.")]
		public bool MakeCamelCase { get; set; }

		[Option("interface", HelpText = "Generate Interfaces instead of classes")]
		public bool MakeInterfaces { get; set; }

		[Option("type-prefix", HelpText = "Prefix for custom type names")]
		public string TypePrefix { get; set; }

		[Option("array-types", HelpText = "Types which are converted to typescript arrays")]
		public IEnumerable<string> ArrayTypes { get; set; }

		[Option("alias-types", HelpText = "Type name alias. In the form <csharp-name>:<typescript-name>")]
		public IEnumerable<string> AliasTypes { get; set; }
	}
}
