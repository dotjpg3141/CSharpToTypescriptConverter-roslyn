using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CSharpToTypescriptConverter.Converter;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpToTypescriptConverter
{
	internal static class Program
	{
		static int Main(string[] args)
		{
			var exitCode = 0;

			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(options =>
				{
					try
					{
						GenerateSource(options);
					}
					catch (IOException e)
					{
						Console.Error.WriteLine(e.Message);
						exitCode = 8;
					}
					catch (SyntaxErrorException e)
					{
						Console.Error.WriteLine(e.Message);
						exitCode = 8;
					}
				})
				.WithNotParsed(errorList => exitCode = 4);

			return exitCode;
		}

		private static void GenerateSource(Options options)
		{
			var inputPaths = new List<string>();
			if (options.InputFiles != null)
			{
				inputPaths.AddRange(options.InputFiles);
			}
			if (options.InputDirectories != null)
			{
				var searchOptions = options.RecursiveInputDirectories
					? SearchOption.AllDirectories
					: SearchOption.TopDirectoryOnly;

				inputPaths.AddRange(options.InputDirectories
					.SelectMany(path => Directory.EnumerateFiles(path, "*.cs", searchOptions)));
			}

			var useStdOut = string.IsNullOrEmpty(options.OutputDirectory);
			var inputFiles = inputPaths.Select(File.ReadAllText).ToArray();
			var output = useStdOut ? (TextWriter)new StringWriter() : new StreamWriter(options.OutputDirectory);

			var generator = new TypeScriptGenerator(output)
			{
				CustomTypePrefix = options.TypePrefix,
				ConvertFieldsToCamelCase = options.MakeCamelCase,
				ConvertClassesToInterfaces = options.MakeInterfaces,
			};

			if (options.Verbose)
			{
				generator.OnUnhandledSyntaxNode += node => Console.Error.WriteLine($"[WARNING] Unhandled syntax node {node.Kind()}");
			}

			generator.ArrayTypeNames.UnionWith(options.ArrayTypes);
			foreach (var typeConversion in options.AliasTypes)
			{
				var types = typeConversion.Split(':');
				var csharp = types.Length >= 1 ? types[0] : "???";
				var typescript = types.Length >= 2 ? types[1] : "???";
				generator.TypeNameConversions[csharp] = typescript;
			}

			generator.Generate(inputFiles);

			if (useStdOut)
			{
				Console.WriteLine(output);
			}
			output.Close();
		}
	}
}