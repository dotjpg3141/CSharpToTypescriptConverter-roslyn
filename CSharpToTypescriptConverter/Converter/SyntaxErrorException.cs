using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpToTypescriptConverter.Converter
{
	public class SyntaxErrorException : Exception
	{
		public SyntaxErrorException(string message) : base(message)
		{
		}

		public SyntaxErrorException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
