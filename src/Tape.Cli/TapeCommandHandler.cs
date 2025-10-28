using BigRedProf.Data.Core;
using BigRedProf.Data.Tape;
using BigRedProf.Data.Tape.Libraries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BigRedProf.Data.Tape.Cli
{
	public static class TapeCommandHandler
	{
		#region methods
		public static int Execute(string[] args)
		{
			if (args == null)
				throw new ArgumentNullException(nameof(args));

			if (args.Length == 0)
			{
				HelpWriter.WriteTapeUsage();
				return 1;
			}

			string subcommand = args[0];

			if (IsHelpOption(subcommand))
			{
				HelpWriter.WriteTapeUsage();
				return 0;
			}

			if (string.Equals(subcommand, "ls", StringComparison.OrdinalIgnoreCase) || string.Equals(subcommand, "list", StringComparison.OrdinalIgnoreCase))
				return ListTapes();

			HelpWriter.WriteUnknownTapeCommand(subcommand);
			return 1;
		}

		private static bool IsHelpOption(string value)
		{
			if (value == null)
				return false;

			if (string.Equals(value, "-h", StringComparison.OrdinalIgnoreCase))
				return true;

			if (string.Equals(value, "--help", StringComparison.OrdinalIgnoreCase))
				return true;

			return false;
		}

		private static int ListTapes()
		{
			try
			{
				string workingDirectory = Environment.CurrentDirectory;
				DiskLibrary library = CreateLibrary(workingDirectory);
				IEnumerable<Tape> tapes = library.Librarian.FetchAllTapes();
				IList<Tape> tapeList = new List<Tape>(tapes);

				Console.WriteLine("Tapes in '{0}':", workingDirectory);

				if (tapeList.Count == 0)
				{
					Console.WriteLine("  (none)");
					return 0;
				}

				foreach (Tape tape in tapeList)
				{
					string description = BuildTapeDescription(tape);
					Console.WriteLine("  {0}", description);
				}

				return 0;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("Failed to list tapes: {0}", ex.Message);
				return 2;
			}
		}

		private static DiskLibrary CreateLibrary(string workingDirectory)
		{
			Debug.Assert(!string.IsNullOrWhiteSpace(workingDirectory));
			return new DiskLibrary(workingDirectory);
		}

		private static string BuildTapeDescription(Tape tape)
		{
			if (tape == null)
				throw new ArgumentNullException(nameof(tape));

			try
			{
				TapeLabel label = tape.ReadLabel();
				return ComposeTapeDescription(tape, label);
			}
			catch (Exception ex)
			{
				return string.Format("{0} (failed to read label: {1})", tape.Id, ex.Message);
			}
		}

		private static string ComposeTapeDescription(Tape tape, TapeLabel label)
		{
			Debug.Assert(tape != null);
			Debug.Assert(label != null);

			Guid tapeId;
			if (!label.TryGetTrait<Guid>(CoreTrait.Id, out tapeId))
				tapeId = tape.Id;

			string displayName = ResolveTapeName(label);
			string seriesText = ResolveSeriesText(label);

			StringBuilder builder = new StringBuilder();
			builder.Append(tapeId);
			builder.Append(' ');
			builder.Append(displayName);

			if (seriesText.Length > 0)
			{
				builder.Append(' ');
				builder.Append(seriesText);
			}

			return builder.ToString();
		}

		private static string ResolveTapeName(TapeLabel label)
		{
			Debug.Assert(label != null);

			string tapeName;
			if (label.TryGetTrait<string>(CoreTrait.Name, out tapeName))
			{
				if (!string.IsNullOrWhiteSpace(tapeName))
					return tapeName;
			}

			return "(unnamed)";
		}

		private static string ResolveSeriesText(TapeLabel label)
		{
			Debug.Assert(label != null);

			Guid seriesId;
			string seriesName;
			int seriesNumber;
			bool hasSeriesId = label.TryGetTrait<Guid>(CoreTrait.SeriesId, out seriesId);
			bool hasSeriesName = label.TryGetTrait<string>(CoreTrait.SeriesName, out seriesName);
			bool hasSeriesNumber = label.TryGetTrait<int>(CoreTrait.SeriesNumber, out seriesNumber);

			if (!hasSeriesId && !hasSeriesName && !hasSeriesNumber)
				return string.Empty;

			StringBuilder builder = new StringBuilder();
			builder.Append('[');

			bool needsSeparator = false;

			if (hasSeriesName)
			{
				builder.Append(seriesName);
				needsSeparator = true;
			}

			if (hasSeriesNumber)
			{
				if (needsSeparator)
					builder.Append(' ');
				builder.Append('#');
				builder.Append(seriesNumber);
				needsSeparator = true;
			}

			if (hasSeriesId)
			{
				if (needsSeparator)
					builder.Append("; ");
				builder.Append(seriesId);
			}

			builder.Append(']');
			return builder.ToString();
		}
		#endregion
	}
}
