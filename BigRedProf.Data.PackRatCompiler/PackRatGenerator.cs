﻿using BigRedProf.Data.PackRatCompiler.Internal;
using BigRedProf.Data.PackRatCompiler.Internal.Symbols;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigRedProf.Data.PackRatCompiler
{
	/// <summary>
	/// This class can generate a <see cref="PackRat"/> class from a given model.
	/// </summary>
	public class PackRatGenerator
	{
		#region fields
		private ICompilationContext _compilationContext;
		#endregion

		#region constructors
		public PackRatGenerator(ICompilationContext compilationContext)
		{
			_compilationContext = compilationContext;
		}
		#endregion

		#region methods
		/// <summary>
		/// Generates a <see cref="PackRat"/> from the specified input model and writes it
		/// to the specified output model stream.
		/// </summary>
		/// <param name="inputModel">The model.</param>
		/// <param name="outputPackRatStream">The generated pack rat.</param>
		public void GeneratePackRat(INamedTypeSymbol inputModel, Stream outputPackRatStream)
		{
			if (inputModel == null)
				throw new ArgumentNullException(nameof(inputModel));

			if (outputPackRatStream == null)
				throw new ArgumentNullException(nameof(outputPackRatStream));

			using (CSharpWriter writer = new CSharpWriter(outputPackRatStream))
			{
				WriteFile(inputModel, writer);
			}
		}
		#endregion

		#region private methods
		private void WriteFile(INamedTypeSymbol modelClass, CSharpWriter writer)
		{
			string @namespace = modelClass.ContainingNamespace.ToDisplayString();

			writer.WriteLine("// <auto-generated/>");
			writer.WriteLine();

			writer.WriteLine("using BigRedProf.Data;");
			writer.WriteLine();

			writer.WriteLine($"namespace {@namespace}");
			writer.WriteOpeningCurlyBrace();
			WritePackRatClass(modelClass, writer);
			writer.WriteClosingCurlyBrace();
		}

		private void WritePackRatClass(INamedTypeSymbol modelClass, CSharpWriter writer)
		{
			string className = modelClass.Name + "PackRat";
			string modelType = modelClass.Name;

			AttributeData generatePackRatAttribute = SymbolHelper.GetAttribute(modelClass, "BigRedProf.Data.GeneratePackRat");
			string schemaId = (string)generatePackRatAttribute.ConstructorArguments[0].Value!;

			IList<PackFieldInfo> fields = SymbolHelper.GetPackRatFields(modelClass);
			ValidatePackRatFields(modelClass, fields);

			writer.WriteLine($"[AssemblyPackRat(\"{schemaId}\")]");
			writer.WriteLine($"public sealed class {className} : PackRat<{modelType}>");
			writer.WriteOpeningCurlyBrace();

			writer.WriteLine($"public {className}(IPiedPiper piedPiper)");
			writer.WriteLine("\t: base(piedPiper)");
			writer.WriteOpeningCurlyBrace();
			writer.WriteClosingCurlyBrace();
			writer.WriteLine();

			writer.WriteLine($"public override void PackModel(CodeWriter writer, {modelType} model)");
			writer.WriteOpeningCurlyBrace();

			for (int i = 0; i < fields.Count; ++i)
			{
				PackFieldInfo field = fields[i];
				WritePackRatFieldPackingCode(modelClass, field, writer);

				if (i != fields.Count - 1)
				{
					writer.WriteLine();
				}
			}
			writer.WriteClosingCurlyBrace(); // PackModel method
			writer.WriteLine();

			writer.WriteLine($"public override {modelType} UnpackModel(CodeReader reader)");
			writer.WriteOpeningCurlyBrace();
			writer.WriteLine($"{modelType} model = new {modelType}();");
			writer.WriteLine();

			for (int i = 0; i < fields.Count; ++i)
			{
				PackFieldInfo field = fields[i];
				WritePackRatFieldUnpackingCode(modelClass, field, writer);
				writer.WriteLine();
			}
			writer.WriteLine("return model;");
			writer.WriteClosingCurlyBrace(); // UnpackModel method

			writer.WriteClosingCurlyBrace(); // class
		}

		private void WritePackRatFieldPackingCode(
			ITypeSymbol modelClass,
			PackFieldInfo field,
			CSharpWriter writer
		)
		{
			writer.WriteLine($"// {field.Name}");

			PackListFieldInfo? packListFieldInfo = field as PackListFieldInfo;
			if(packListFieldInfo != null)
			{
				WritePackRatListFieldPackingCode(modelClass, packListFieldInfo, writer);
				return;
			}

			string packType = field.IsEnum ? "int" : field.Type!;

			if (field.IsNullable)
			{
				writer.WriteLine(
					$"PiedPiper.PackNullableModel<{packType}>(" +
					$"writer, model.{field.Name}, \"{field.SchemaId}\", " +
					((field.ByteAligned == ByteAligned.Yes) ? "ByteAligned.Yes" : "ByteAligned.No") +
					$");"
				);
			}
			else
			{
				if (field.ByteAligned == ByteAligned.Yes)
					writer.WriteLine("writer.AlignToByteBoundary();");
				writer.WriteLine($"PiedPiper.GetPackRat<{packType}>(\"{field.SchemaId}\")");
				if(field.IsEnum)
					writer.WriteLine($"\t.PackModel(writer, (int) model.{field.Name});");
				else
					writer.WriteLine($"\t.PackModel(writer, model.{field.Name});");
			}
		}

		private void WritePackRatListFieldPackingCode(
			ITypeSymbol modelClass,
			PackListFieldInfo field,
			CSharpWriter writer
		)
		{
			writer.WriteLine($"PiedPiper.PackList<{field.Type}>(");
			writer.IncreaseIndentation();
			writer.WriteLine("writer,");
			writer.WriteLine($"model.{field.Name},");
			writer.WriteLine($"\"{field.ElementSchemaId}\",");
			writer.WriteLine($"{field.IsNullable.ToString().ToLower()},");
			writer.WriteLine($"{field.IsElementNullable.ToString().ToLower()},");
			if (field.ByteAligned == ByteAligned.Yes)
				writer.WriteLine("ByteAligned.Yes");
			else
				writer.WriteLine("ByteAligned.No");
			writer.DecreaseIndentation();
			writer.WriteLine(");");
		}

		private void WritePackRatFieldUnpackingCode(
			ITypeSymbol modelClass,
			PackFieldInfo field,
			CSharpWriter writer
		)
		{
			writer.WriteLine($"// {field.Name}");

			PackListFieldInfo? packListFieldInfo = field as PackListFieldInfo;
			if (packListFieldInfo != null)
			{
				WritePackRatListFieldUnpackingCode(modelClass, packListFieldInfo, writer);
				return;
			}

			if (field.ByteAligned == ByteAligned.Yes)
				writer.WriteLine("reader.AlignToByteBoundary();");

			string packType = field.IsEnum ? "int" : field.Type!;

			if (field.IsNullable)
			{
				writer.WriteLine($"model.{field.Name} = PiedPiper.UnpackNullableModel<{packType}>("
					+ "reader, " +
					$"\"{field.SchemaId}\", " +
					(field.ByteAligned == ByteAligned.Yes ? "ByteAligned.Yes" : "ByteAligned.No") +
					");"
				);
			}
			else
			{
				if(field.IsEnum)
					writer.WriteLine($"model.{field.Name} = ({field.Type}) PiedPiper.GetPackRat<{packType}>(\"{field.SchemaId}\")");
				else
					writer.WriteLine($"model.{field.Name} = PiedPiper.GetPackRat<{packType}>(\"{field.SchemaId}\")");
				writer.WriteLine($"\t.UnpackModel(reader);");
			}
		}

		private void WritePackRatListFieldUnpackingCode(
			ITypeSymbol modelClass,
			PackListFieldInfo field,
			CSharpWriter writer
		)
		{
			writer.WriteLine($"model.{field.Name} = PiedPiper.UnpackList<{field.Type}>(");
			writer.IncreaseIndentation();
			writer.WriteLine($"reader,");
			writer.WriteLine($"\"{field.ElementSchemaId}\",");
			writer.WriteLine($"{field.IsNullable.ToString().ToLower()},");
			writer.WriteLine($"{field.IsElementNullable.ToString().ToLower()},");
			if (field.ByteAligned == ByteAligned.Yes)
				writer.WriteLine("ByteAligned.Yes");
			else
				writer.WriteLine("ByteAligned.No");
			writer.DecreaseIndentation();
			writer.WriteLine(");");
		}

		private void ValidatePackRatFields(INamedTypeSymbol modelClass, IList<PackFieldInfo> fields)
		{
			for (int i = 0; i < fields.Count; ++i)
			{
				PackFieldInfo field = fields[i];

				if (field.Position != i + 1)
				{
					_compilationContext.ReportError(
						CompilerError.InvalidFieldPosition,
						String.Format(
							"Field '{0}' in model '{1}' has invalid field position. Expected: {2}. Actual: {3}",
							field.Name,
							modelClass.ToDisplayString(),
							i + 1,
							field.Position
						),
						modelClass.ContainingModule.ToDisplayString(),
						field.SourceLineNumber,
						field.SourceColumn
					);
				}
			}
		}
		#endregion
	}
}