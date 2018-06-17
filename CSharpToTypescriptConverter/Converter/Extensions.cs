using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpToTypescriptConverter.Converter
{
	public static class Extensions
	{
		public static void Consume<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var item in source)
			{
				action(item);
			}
		}

		public static void AcceptAll<T>(this IEnumerable<T> source, CSharpSyntaxVisitor visitor)
			where T : CSharpSyntaxNode
		{
			foreach (var item in source)
			{
				item.Accept(visitor);
			}
		}
	}
}
