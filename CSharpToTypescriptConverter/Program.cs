using System;
using System.IO;
using System.Linq;
using CommandLine;
using CSharpToTypescriptConverter.Converter;

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
						Console.Error.WriteLine(e);
						exitCode = 8;
					}
				})
				.WithNotParsed(errorList => exitCode = 4);

			return exitCode;
		}

		private static void GenerateSource(Options options)
		{
			var useStdOut = string.IsNullOrEmpty(options.Output);
			var inputFiles = options.InputFiles.Select(File.ReadAllText).ToArray();
			var output = useStdOut ? Console.Out : new StreamWriter(options.Output);

			var generator = new TypeScriptGenerator(output)
			{
				CustomTypePrefix = options.TypePrefix,
				ConvertFieldsToCamelCase = options.MakeCamelCase,
				ConvertClassesToInterfaces = options.MakeInterfaces,
			};

			generator.ArrayTypeNames.UnionWith(options.ArrayTypes);
			foreach (var typeConversion in options.AliasTypes)
			{
				var types = typeConversion.Split(':');
				var csharp = types.Length >= 1 ? types[0] : "???";
				var typescript = types.Length >= 2 ? types[1] : "???";
				generator.TypeNameConversions[csharp] = typescript;
			}

			generator.Generate(inputFiles);

			if (!useStdOut)
			{
				output.Close();
			}
		}
	}
}