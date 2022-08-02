using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigRedProf.Data.PackRatCompiler.Internal
{
	[RegisterPackRat("123")]
	internal class Compiler
	{
		#region fields
		private PackRatGenerator _packRatGenerator;
		#endregion

		#region methods
		public void Compile(CommandLineOptions options)
		{
			Debug.Assert(options != null);
			Debug.Assert(options.InputPath != null);
			Debug.Assert(options.OutputPath != null);

			_packRatGenerator = new PackRatGenerator();

			DirectoryInfo inputDirectory = new DirectoryInfo(options.InputPath);
			Debug.Assert(inputDirectory.Exists);

			DirectoryInfo outputDirectory = new DirectoryInfo(options.OutputPath);
			if (!outputDirectory.Exists)
				outputDirectory.Create();

			foreach(FileInfo inputFile in inputDirectory.GetFiles("*.cs", SearchOption.AllDirectories))
			{
				// HACKHACK: this isn't a great way to do this
				if (inputFile.FullName.Contains("\\obj\\"))
					continue;

				// HACKHACK: this isn't a great way to do this
				if (inputFile.FullName.Contains("\\bin\\"))
					continue;

				// HACKHACK: this isn't a great way to do this
				FileInfo outputFile = new FileInfo(
						Path.Combine(outputDirectory.FullName, inputFile.Name)
						.Replace(".cs", ".g.cs")
					);

				ProcessFile(inputFile, outputFile);
			}
		}
		#endregion

		#region private methods
		private void ProcessFile(FileInfo inputFile, FileInfo outputFile)
		{
			using(FileStream inputStream = new FileStream(inputFile.FullName, FileMode.Open, FileAccess.Read))
			using (FileStream outputStream = new FileStream(outputFile.FullName, FileMode.Create, FileAccess.Write))
			{
				_packRatGenerator.GeneratePackRat(inputStream, outputStream);
			}
		}
		#endregion
	}
}
