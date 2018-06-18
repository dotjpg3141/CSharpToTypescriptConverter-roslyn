using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToTypescriptConverter.Converter
{
	public class TypeScriptGenerator
	{
		public HashSet<string> ArrayTypeNames { get; }
		public Dictionary<string, string> TypeNameConversions { get; }
		public bool ConvertFieldsToCamelCase { get; set; }
		public bool ConvertClassesToInterfaces { get; set; }
		public string CustomTypePrefix { get; set; }
		public bool MakeEnumConst { get; set; }

		public Action<SyntaxNode> OnUnhandledSyntaxNode;

		private readonly IndentWriter writer;
		private GeneratorMode mode;
		private Dictionary<string, CombinedClass> collectedClasses;

		public TypeScriptGenerator(TextWriter writer)
		{
			this.writer = new IndentWriter(writer);
			this.ArrayTypeNames = new HashSet<string>(){
				"List",
				"IList",
				"ICollection",
				"IEnumerable",
				"IReadOnlyList",
				"IReadOnlyCollection",
			};

			this.TypeNameConversions = new Dictionary<string, string>()
			{
				["Boolean"] = "boolean",
				["Byte"] = "number",
				["Int16"] = "number",
				["Int32"] = "number",
				["Int64"] = "number",
				["UInt16"] = "number",
				["UInt32"] = "number",
				["UInt64"] = "number",
				["Single"] = "number",
				["Double"] = "number",
				["Decimal"] = "number",
				["String"] = "string",
				["Object"] = "object",
				["DateTime"] = "Date",
			};
		}

		public void Generate(params string[] csharpCode)
		{
			this.collectedClasses = new Dictionary<string, CombinedClass>();

			this.mode = GeneratorMode.CollectClasses;
			foreach (var code in csharpCode)
			{
				var tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(code);

				var errorList = tree.GetDiagnostics().Where(f => f.Severity == DiagnosticSeverity.Error).ToList();
				if (errorList.Count != 0)
				{
					throw new SyntaxErrorException(string.Join(Environment.NewLine, errorList));
				}

				VisitCompilationUnit((CompilationUnitSyntax)tree.GetRoot());
			}

			this.mode = GeneratorMode.GenerateCode;
			this.collectedClasses.Values.Consume(VisitCombinedClass);
		}

		private void VisitCompilationUnit(CompilationUnitSyntax compilationUnit)
		{
			compilationUnit.Members.Consume(VisitMemberDeclaration);
		}

		private void VisitMemberDeclaration(MemberDeclarationSyntax memberDeclaration)
		{
			switch (memberDeclaration)
			{
				case NamespaceDeclarationSyntax namespaceDecl: VisitNamespace(namespaceDecl); break;
				case ClassDeclarationSyntax classDecl: VisitClassDeclaration(classDecl); break;
				case PropertyDeclarationSyntax propertyDecl: VisitPropertyDeclaration(propertyDecl); break;
				case EnumDeclarationSyntax enumDecl: VisitEnumDelcaration(enumDecl); break;
				default:
					this.OnUnhandledSyntaxNode?.Invoke(memberDeclaration);
					break;
			}
		}

		private void VisitNamespace(NamespaceDeclarationSyntax namespaceDecl)
		{
			namespaceDecl.Members.Consume(VisitMemberDeclaration);
		}

		private void VisitClassDeclaration(ClassDeclarationSyntax classDecl)
		{
			if (IgnoreByModifier(classDecl.Modifiers))
				return;

			if (this.mode == GeneratorMode.CollectClasses)
			{
				var className = classDecl.Identifier.Text;

				if (!this.collectedClasses.TryGetValue(className, out var combinedClass))
				{
					combinedClass = new CombinedClass()
					{
						CSharpName = className,
						ClassDeclarations = new List<ClassDeclarationSyntax>(),
					};
					this.collectedClasses[className] = combinedClass;
				}

				combinedClass.ClassDeclarations.Add(classDecl);
			}
		}

		private void VisitCombinedClass(CombinedClass @class)
		{
			@class.ClassDeclarations.SelectMany(c => c.AttributeLists).Consume(VisitAttributeList);
			VisitXmlComment(@class.ClassDeclarations.SelectMany(c => c.GetLeadingTrivia()));

			var classType = this.ConvertClassesToInterfaces ? "interface" : "class";
			var identifier = RewriteTypeName(@class.CSharpName);
			this.writer.WriteLine($"export {classType} {identifier} {{");
			this.writer.Indent();

			@class.ClassDeclarations.SelectMany(c => c.Members).Consume(VisitMemberDeclaration);

			this.writer.Unindent();
			this.writer.WriteLine("}");
		}

		private void VisitEnumDelcaration(EnumDeclarationSyntax enumDecl)
		{
			if (IgnoreByModifier(enumDecl.Modifiers))
				return;

			enumDecl.AttributeLists.Consume(VisitAttributeList);
			VisitXmlComment(enumDecl.GetLeadingTrivia());

			var @const = this.MakeEnumConst ? "const" : "";
			var name = enumDecl.Identifier;

			this.writer.WriteLine($"export {@const} enum {name} {{");
			this.writer.Indent();
			foreach (var enumMember in enumDecl.Members)
			{
				this.writer.BeginLine();
				this.writer.Write(enumMember.Identifier);
				if (enumMember.EqualsValue != null)
				{
					if (enumMember.EqualsValue.Value is LiteralExpressionSyntax literal
						&& literal.Kind() == SyntaxKind.NumericLiteralExpression
						&& long.TryParse(literal.Token.Text, out var constValue))
					{
						this.writer.Write(" = ");
						this.writer.Write(constValue);
					}
					else
					{
						this.OnUnhandledSyntaxNode?.Invoke(enumMember.EqualsValue.Value);
						this.writer.Write("/*");
						this.writer.Write(enumMember.EqualsValue.Value);
						this.writer.Write("*/");
					}
				}
				this.writer.Write(",");
				this.writer.EndLine();
			}
			this.writer.Unindent();
			this.writer.WriteLine("}");
		}

		private void VisitPropertyDeclaration(PropertyDeclarationSyntax propertyDecl)
		{
			if (IgnoreByModifier(propertyDecl.Modifiers))
				return;

			var getter = propertyDecl.AccessorList.Accessors.FirstOrDefault(accessor => accessor.Kind() == SyntaxKind.GetAccessorDeclaration);
			if (getter == null || IgnoreByModifier(getter.Modifiers, SyntaxKind.PublicKeyword))
				return;

			propertyDecl.AttributeLists.Consume(VisitAttributeList);
			VisitXmlComment(propertyDecl.GetLeadingTrivia());

			var name = RewriteMemberName(propertyDecl.Identifier.Text);

			this.writer.BeginLine();
			this.writer.Write($"public {name}: ");
			this.VisitType(propertyDecl.Type);
			this.writer.Write(";");
			this.writer.EndLine();
		}

		private void VisitType(TypeSyntax type)
		{
			switch (type)
			{
				case PredefinedTypeSyntax predefinedType: VisitPredefinedType(predefinedType); break;
				case IdentifierNameSyntax identifierName: VisitIdentifierName(identifierName); break;
				case ArrayTypeSyntax arrayType: VisitArrayTypeSyntax(arrayType); break;
				case GenericNameSyntax genericName: VisitGenericName(genericName); break;
				default:
					this.OnUnhandledSyntaxNode?.Invoke(type);
					this.writer.Write("any");
					break;
			}
		}

		private void VisitPredefinedType(PredefinedTypeSyntax predefinedType)
		{
			switch (predefinedType.Keyword.Kind())
			{
				case SyntaxKind.BoolKeyword:
					this.writer.Write("boolean");
					break;

				case SyntaxKind.ByteKeyword:
				case SyntaxKind.SByteKeyword:
				case SyntaxKind.ShortKeyword:
				case SyntaxKind.UShortKeyword:
				case SyntaxKind.IntKeyword:
				case SyntaxKind.UIntKeyword:
				case SyntaxKind.LongKeyword:
				case SyntaxKind.ULongKeyword:
				case SyntaxKind.DoubleKeyword:
				case SyntaxKind.FloatKeyword:
				case SyntaxKind.DecimalKeyword:
					this.writer.Write("number");
					break;

				case SyntaxKind.StringKeyword:
				case SyntaxKind.CharKeyword:
					this.writer.Write("string");
					break;

				case SyntaxKind.VoidKeyword:
					this.writer.Write("void");
					break;

				case SyntaxKind.ObjectKeyword:
					this.writer.Write("any");
					break;

				default:
					this.OnUnhandledSyntaxNode?.Invoke(predefinedType);
					this.writer.Write(predefinedType.Keyword.Text);
					break;
			}
		}

		private void VisitIdentifierName(IdentifierNameSyntax identifierName)
		{
			if (this.ArrayTypeNames.Contains(identifierName.Identifier.Text))
			{
				this.writer.Write("any[]");
			}
			else
			{
				var name = RewriteTypeName(identifierName.Identifier.Text);
				this.writer.Write(name);
			}
		}

		private void VisitArrayTypeSyntax(ArrayTypeSyntax arrayType)
		{
			VisitType(arrayType.ElementType);
			this.writer.Write("[]");
		}

		private void VisitGenericName(GenericNameSyntax genericName)
		{
			string name = genericName.Identifier.Text;
			if (this.ArrayTypeNames.Contains(name) && genericName.TypeArgumentList.Arguments.Count == 1)
			{
				VisitType(genericName.TypeArgumentList.Arguments[0]);
				this.writer.Write("[]");
			}
			else
			{
				this.writer.Write(RewriteTypeName(name));
				VisitTypeArgumentList(genericName.TypeArgumentList);
			}
		}

		private void VisitTypeArgumentList(TypeArgumentListSyntax typeArguments)
		{
			this.writer.Write("<");
			bool first = true;
			foreach (var argument in typeArguments.Arguments)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					this.writer.Write(",");
				}

				VisitType(argument);
			}
			this.writer.Write(">");
		}

		private string RewriteTypeName(string csharpTypeName)
		{
			if (this.collectedClasses.ContainsKey(csharpTypeName))
			{
				return this.CustomTypePrefix + csharpTypeName;
			}
			if (this.TypeNameConversions.TryGetValue(csharpTypeName, out var typescriptTypeName))
			{
				return typescriptTypeName;
			}
			return csharpTypeName;
		}

		private string RewriteMemberName(string csharpMemberName)
		{
			if (this.ConvertFieldsToCamelCase && csharpMemberName.Length > 0)
			{
				return char.ToLowerInvariant(csharpMemberName[0]) + csharpMemberName.Substring(1);
			}

			return csharpMemberName;
		}

		private bool IgnoreByModifier(SyntaxTokenList modifiers, SyntaxKind defaultModifier = SyntaxKind.InternalKeyword)
		{
			var modifierKinds = modifiers.Select(m => m.Kind()).ToImmutableHashSet();

			if (modifierKinds.Contains(SyntaxKind.StaticKeyword))
				return true;

			if (modifierKinds.Contains(SyntaxKind.PrivateKeyword) ||
				modifierKinds.Contains(SyntaxKind.ProtectedKeyword) ||
				modifierKinds.Contains(SyntaxKind.InternalKeyword))
				return true;

			return !modifierKinds.Contains(SyntaxKind.PublicKeyword)
				&& defaultModifier != SyntaxKind.PublicKeyword;
		}

		private void VisitXmlComment(IEnumerable<SyntaxTrivia> trivia)
		{
			var comments = trivia.Where(
					t => t.Kind() == SyntaxKind.SingleLineDocumentationCommentTrivia
					  || t.Kind() == SyntaxKind.MultiLineDocumentationCommentTrivia)
				.Select(c => c.ToFullString())
				.ToList();

			if (comments.Any())
			{
				var lines = string.Join("\n", comments)
					.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(line => "* " + line.Trim().TrimStart('/', '*'))
					.ToList();

				var comment = $"/**{Environment.NewLine}{string.Join(Environment.NewLine, lines)}{Environment.NewLine}*/";
				this.writer.WriteLine(comment);
			}
		}

		private void VisitAttributeList(AttributeListSyntax attributeList)
		{
			foreach (var attribute in attributeList.Attributes)
			{
				var attributeText = new Regex(@"[\r\n]+")
					.Replace(attribute.ToFullString(), Environment.NewLine + "//");

				this.writer.WriteLine($"//{attributeText}");
			}
		}

		private enum GeneratorMode
		{
			CollectClasses,
			GenerateCode,
		}
	}
}