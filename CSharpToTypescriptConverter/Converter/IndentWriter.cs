using System;
using System.IO;

namespace CSharpToTypescriptConverter.Converter
{
	public class IndentWriter
	{
		public TextWriter Writer { get; }
		public int IndentLevel { get; set; }
		public string IndentString { get; set; }

		public IndentWriter(TextWriter writer)
		{
			this.Writer = writer ?? throw new ArgumentNullException(nameof(writer));
			this.IndentString = "\t";
		}

		public void BeginLine()
		{
			for (int i = 0; i < this.IndentLevel; i++)
			{
				this.Writer.Write(this.IndentString);
			}
		}

		public void EndLine()
		{
			this.Writer.WriteLine();
		}

		public void Write(string value)
		{
			this.Writer.Write(value);
		}

		public void Write(object value)
		{
			this.Write(value?.ToString() ?? "");
		}

		public void WriteLine()
		{
			this.BeginLine();
			this.EndLine();
		}

		public void WriteLine(string value)
		{
			this.BeginLine();
			this.Write(value);
			this.EndLine();
		}

		public void WriteLine(object value)
		{
			this.WriteLine(value?.ToString() ?? "");
		}

		public void Indent()
		{
			this.IndentLevel++;
		}

		public void Unindent()
		{
			this.IndentLevel--;
		}
	}
}
