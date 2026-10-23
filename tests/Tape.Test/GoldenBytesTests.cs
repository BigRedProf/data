using BigRedProf.Data.Core;
using BigRedProf.Data.Tape;
using BigRedProf.Data.Tape.Providers.Memory;
using System.Globalization;

namespace BigRedProf.Data.Tape.Test
{
	/// <summary>
	/// Replays tests/Golden/golden.txt, captured from CodeWriter before this change.
	/// The bytes in that file are the oracle.
	/// </summary>
	public class GoldenBytesTests
	{
		#region tests
		[Fact]
		public void CapturedBytes_AreReproducedAndReadBack()
		{
			string path = Path.Combine(AppContext.BaseDirectory, "Golden", "golden.txt");
			string text = File.ReadAllText(path);
			List<Sample> samples = Parse(text);

			Assert.Contains(samples, sample => sample.Name == "raw-mid-byte");
			Assert.Contains(samples, sample => sample.Name == "raw-zero-bit");
			Assert.Contains(samples, sample => sample.Name == "raw-back-to-back");
			Assert.Contains(samples, sample => sample.Name == "raw-align-example");
			Assert.Contains(samples, sample => sample.Name == "tape-multi-session-unaligned");
			Assert.Contains(samples, sample => sample.Name == "packrat-code-mid-byte");
			Assert.Contains(samples, sample => sample.Name == "packrat-code-zero-bit");

			PiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
			piedPiper.DefineCoreTraits();

			foreach (Sample sample in samples)
			{
				if (sample.Kind == "raw")
					ReplayRaw(sample);
				else if (sample.Kind == "packrat")
					ReplayPackRat(piedPiper, sample);
				else if (sample.Kind == "tape")
					ReplayTape(sample);
				else
					throw new InvalidOperationException("Unknown kind '" + sample.Kind + "' in " + sample.Name + ".");
			}
		}
		#endregion

		#region replay
		private static void ReplayRaw(Sample sample)
		{
			int bitLength = 0;
			List<Action<BitStream>> reads = new List<Action<BitStream>>();
			MemoryStream stream = new MemoryStream();
			using (StreamBitWriter writer = new StreamBitWriter(stream, leaveOpen: true))
			{
				foreach (string operation in sample.Operations)
				{
					if (operation == "ALIGN")
					{
						writer.AlignToNextByteBoundary();
						int remainder = bitLength % 8;
						if (remainder != 0)
							bitLength += 8 - remainder;

						int expectedPosition = bitLength;
						reads.Add(
							delegate(BitStream reader)
							{
								reader.AlignToNextByteBoundary();
								Assert.Equal(expectedPosition, reader.Position);
							}
						);
					}
					else if (operation == "FLUSH")
					{
						writer.Flush();
						reads.Add(
							delegate(BitStream reader)
							{
								reader.Flush();
							}
						);
					}
					else if (operation.StartsWith("WRITE ", StringComparison.Ordinal))
					{
						Code code = CodeFromOperation(operation);
						writer.WriteBits(code);
						bitLength += code.Length;

						Code expectedCode = code;
						int expectedPosition = bitLength;
						reads.Add(
							delegate(BitStream reader)
							{
								Code actual = reader.ReadBits(expectedCode.Length);
								Assert.Equal(expectedCode, actual);
								Assert.Equal(expectedPosition, reader.Position);
							}
						);
					}
					else
					{
						throw new InvalidOperationException(sample.Name + " has unknown operation '" + operation + "'.");
					}

					Assert.Equal(bitLength, writer.Position);
				}
			}

			Assert.Equal(sample.BitLength, bitLength);
			Assert.Equal(ParseHex(sample.Hex), stream.ToArray());

			using (StreamBitReader reader = new StreamBitReader(new MemoryStream(ParseHex(sample.Hex))))
			{
				foreach (Action<BitStream> read in reads)
					read(reader);
			}
		}

		private static void ReplayPackRat(PiedPiper piedPiper, Sample sample)
		{
			object model = BuildModel(piedPiper, sample.Type, sample.Value);
			Code exact = piedPiper.PackModel(model, sample.Schema);
			byte[] golden = ParseHex(sample.Hex);

			Assert.Equal(sample.BitLength, exact.Length);
			Assert.Equal(golden, exact.ToByteArray());

			MemoryStream stream = new MemoryStream();
			using (StreamBitWriter writer = new StreamBitWriter(stream, leaveOpen: true))
			{
				piedPiper.PackModel(writer, model, sample.Schema);
				Assert.Equal(sample.BitLength, writer.Position);
			}

			Assert.Equal(golden, stream.ToArray());

			using (StreamBitReader reader = new StreamBitReader(new MemoryStream(golden)))
			{
				object unpacked = piedPiper.UnpackModel(reader, sample.Schema);
				AssertSameModel(sample.Type, sample.Value, model, unpacked);
				Assert.Equal(sample.BitLength, reader.Position);
			}
		}

		private static void ReplayTape(Sample sample)
		{
			MemoryTapeProvider provider = new MemoryTapeProvider();
			Librarian librarian = new Librarian(provider);
			Guid seriesId = Guid.Parse(sample.SeriesId);

			bool firstSession = true;
			foreach (List<string> session in sample.Sessions)
			{
				BackupWizard wizard = firstSession
					? BackupWizard.CreateNew(librarian, seriesId, sample.SeriesName, sample.Description)
					: BackupWizard.OpenExisting(librarian, seriesId);
				firstSession = false;

				foreach (string operation in session)
				{
					if (!operation.StartsWith("WRITE ", StringComparison.Ordinal))
						throw new InvalidOperationException(sample.Name + " session operation '" + operation + "' is not a write.");

					wizard.Append(CodeFromOperation(operation));
				}

				wizard.Writer.Dispose();
			}

			IList<Tape> tapes = librarian.FetchTapesInSeries(seriesId);
			Assert.Equal(sample.Tapes.Count, tapes.Count);
			for (int i = 0; i < tapes.Count; ++i)
			{
				Tape tape = tapes[i];
				TapeRecord expected = sample.Tapes[i];
				Assert.Equal(expected.SeriesNumber, tape.ReadLabel().SeriesNumber);
				Assert.Equal(expected.BitLength, tape.Position);

				int byteLength = (expected.BitLength + 7) / 8;
				byte[] content = byteLength == 0
					? Array.Empty<byte>()
					: provider.ReadTapeInternal(tape.Id, 0, byteLength);
				Assert.Equal(ParseHex(expected.Hex), content);
			}

			using (TapeSeriesBitStream reader = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Read))
			{
				foreach (List<string> session in sample.Sessions)
				{
					foreach (string operation in session)
					{
						Code expected = CodeFromOperation(operation);
						Code actual = reader.ReadBits(expected.Length);
						Assert.Equal(expected, actual);
					}
				}
			}
		}
		#endregion

		#region models
		private static object BuildModel(PiedPiper piedPiper, string type, string value)
		{
			if (type == "bool")
				return bool.Parse(value);

			if (type == "byte")
				return byte.Parse(value, CultureInfo.InvariantCulture);

			if (type == "code")
				return value == "-" ? new Code(0) : new Code(value);

			if (type == "datetime")
			{
				string[] parts = value.Split('|');
				long ticks = long.Parse(parts[0], CultureInfo.InvariantCulture);
				DateTimeKind kind = Enum.Parse<DateTimeKind>(parts[1]);
				return new DateTime(ticks, kind);
			}

			if (type == "double")
				return double.Parse(value, CultureInfo.InvariantCulture);

			if (type == "ewn31" || type == "int32" || type == "uvarint" || type == "varint" || type == "wholenumber")
				return int.Parse(value, CultureInfo.InvariantCulture);

			if (type == "int64")
				return long.Parse(value, CultureInfo.InvariantCulture);

			if (type == "single")
				return float.Parse(value, CultureInfo.InvariantCulture);

			if (type == "text")
				return value;

			if (type == "guid")
				return Guid.Parse(value);

			if (type == "datum")
			{
				string[] parts = value.Split('|');
				Code code = parts[1] == "-" ? new Code(0) : new Code(parts[1]);
				return new Datum(parts[0], code);
			}

			if (type == "flexdatum-name")
			{
				return new FlexDatumBuilder(piedPiper)
					.AddTrait<string>(CoreTrait.Name, value)
					.Build();
			}

			if (type == "multihash-sha256")
				return Multihash.FromBytes(ParseHex(value), MultihashAlgorithm.Sha256);

			if (type == "texttrail")
				return TextTrail.FromStringRepresentation(value, '/');

			throw new InvalidOperationException("Unknown pack-rat type '" + type + "'.");
		}

		private static void AssertSameModel(string type, string value, object expected, object actual)
		{
			if (type == "texttrail")
			{
				Assert.Equal(value, actual.ToString());
				return;
			}

			Assert.Equal(expected, actual);
		}

		private static Code CodeFromOperation(string operation)
		{
			string bits = operation.Substring("WRITE ".Length);
			if (bits == "-")
				return new Code(0);

			return new Code(bits);
		}
		#endregion

		#region parsing
		private static List<Sample> Parse(string text)
		{
			List<Sample> samples = new List<Sample>();
			Sample? current = null;
			List<string>? session = null;

			string[] lines = text.Split('\n');
			for (int i = 0; i < lines.Length; ++i)
			{
				string line = lines[i].TrimEnd('\r');
				if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
					continue;

				if (line.StartsWith("SAMPLE ", StringComparison.Ordinal))
				{
					current = new Sample(line.Substring("SAMPLE ".Length));
					session = null;
					continue;
				}

				if (current == null)
					throw new InvalidOperationException("Line outside a sample: " + line);

				if (line == "END")
				{
					samples.Add(current);
					current = null;
					session = null;
					continue;
				}

				if (line == "SESSION")
				{
					session = new List<string>();
					current.Sessions.Add(session);
					continue;
				}

				if (line.StartsWith("OP ", StringComparison.Ordinal))
				{
					string operation = line.Substring("OP ".Length);
					if (session != null)
						session.Add(operation);
					else
						current.Operations.Add(operation);

					continue;
				}

				if (line.StartsWith("TAPE ", StringComparison.Ordinal))
				{
					string[] parts = line.Split(' ');
					TapeRecord record = new TapeRecord();
					record.SeriesNumber = int.Parse(parts[1], CultureInfo.InvariantCulture);
					record.BitLength = int.Parse(parts[3], CultureInfo.InvariantCulture);
					record.Hex = parts[5];
					current.Tapes.Add(record);
					continue;
				}

				int space = line.IndexOf(' ');
				string key = space < 0 ? line : line.Substring(0, space);
				string rest = space < 0 ? "" : line.Substring(space + 1);
				if (key == "KIND")
					current.Kind = rest;
				else if (key == "BIT_LENGTH")
					current.BitLength = int.Parse(rest, CultureInfo.InvariantCulture);
				else if (key == "HEX")
					current.Hex = rest;
				else if (key == "SCHEMA")
					current.Schema = rest;
				else if (key == "TYPE")
					current.Type = rest;
				else if (key == "VALUE")
					current.Value = rest;
				else if (key == "SERIES")
					current.SeriesId = rest;
				else if (key == "NAME")
					current.SeriesName = rest;
				else if (key == "DESCRIPTION")
					current.Description = rest;
				else
					throw new InvalidOperationException("Unknown golden line '" + line + "'.");
			}

			if (current != null)
				throw new InvalidOperationException("Sample " + current.Name + " is missing END.");

			return samples;
		}

		private static byte[] ParseHex(string hex)
		{
			if (hex == "-")
				return Array.Empty<byte>();

			if ((hex.Length % 2) != 0)
				throw new InvalidOperationException("Hex length is odd.");

			byte[] bytes = new byte[hex.Length / 2];
			for (int i = 0; i < bytes.Length; ++i)
				bytes[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

			return bytes;
		}
		#endregion

		#region nested types
		private sealed class Sample
		{
			public Sample(string name)
			{
				Name = name;
				Kind = "";
				Hex = "";
				Schema = "";
				Type = "";
				Value = "";
				SeriesId = "";
				SeriesName = "";
				Description = "";
				Operations = new List<string>();
				Sessions = new List<List<string>>();
				Tapes = new List<TapeRecord>();
			}

			public string Name { get; }
			public string Kind { get; set; }
			public int BitLength { get; set; }
			public string Hex { get; set; }
			public string Schema { get; set; }
			public string Type { get; set; }
			public string Value { get; set; }
			public string SeriesId { get; set; }
			public string SeriesName { get; set; }
			public string Description { get; set; }
			public List<string> Operations { get; }
			public List<List<string>> Sessions { get; }
			public List<TapeRecord> Tapes { get; }
		}

		private sealed class TapeRecord
		{
			public int SeriesNumber { get; set; }
			public int BitLength { get; set; }
			public string Hex { get; set; } = "";
		}
		#endregion
	}
}
