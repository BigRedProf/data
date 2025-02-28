﻿using BigRedProf.Data.Tape.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace BigRedProf.Data.Tape.PackRats
{
	/// <summary>
	/// This custom pack rat for <see cref="TapeHeader"/> needs to ensure:
	/// 1. The packed length is exactly 125 bytes (to allow fixed offset for Label).
	/// 2. It begins with a human-readable string for "cat *" commands.
	/// </summary>
	public class TapeHeaderPackRat : PackRat<TapeHeader>
	{
		#region constructors
		public TapeHeaderPackRat(IPiedPiper piedPiper)
			: base(piedPiper) 
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, TapeHeader model)
		{
			// let the first 100 bytes be human-readable for cat/head CLI commands
			// Format: TAPE|v1|Series-Name|Series-Number|Guid
			// Max Bytes: 4|2|50|4|16
			// Total Bytes: 4 + 2 + 70 + 4 + 16 + 4 (pipes) = 100
			// Then do 0x1A so Windows will automatically stop printing (Linux can
			// use "head -c 100 *"); 101 total
			// Then use 4 bytes for Version; 105 total
			// And 4 bytes for BytesAllocatedForLabel; 109 total
			// And then skip the final 16 bytes (we like the number 125 in this library :)

			// "TAPE" (for correctness)
			Code packedTape = PiedPiper.EncodeModel<string>("TAPE|", CoreSchema.TextAscii);
			writer.WriteCode(packedTape);

			// "v1" (for show)
			string versionText = model.Version.ToString();
			if (!(model.Version >= 1 && model.Version <= 9))
				versionText = "X";
			Code packedVersion = PiedPiper.EncodeModel<string>($"v{versionText}|", CoreSchema.TextAscii);
			writer.WriteCode(packedVersion);

			// series name (for show)
			string? seriesName = null;
			model.Label.TryGetTrait(CoreTrait.SeriesName, out seriesName);
			string terminalFriendlySeriesName = MakeFixedSizeAscii(seriesName, 70);
			PiedPiper.PackModel<string>(writer, terminalFriendlySeriesName + "|", CoreSchema.TextAscii);

			// series number (for show)
			int seriesNumber = 0;
			model.Label.TryGetTrait<int>(CoreTrait.SeriesNumber, out seriesNumber);
			string terminalFriendlySeriesNumber = Make5DigitNumber(seriesNumber);
			PiedPiper.PackModel<string>(writer, terminalFriendlySeriesNumber + "|", CoreSchema.TextAscii);

			// GUID (for show)
			Guid guid = Guid.Empty;
			if (!model.Label.TryGetTrait<Guid>(CoreTrait.Guid, out guid))
			PiedPiper.PackModel<Guid>(writer, guid, CoreSchema.Guid);

			// End-of-file (for show)
			PiedPiper.PackModel<string>(writer, new string((char) 0x1A, 1), CoreSchema.TextAscii);

			// Version (actual)
			PiedPiper.PackModel<int>(writer, model.Version, CoreSchema.Int32);

			// Bytes Allocated For Label (actual)
			PiedPiper.PackModel<int>(writer, model.BytesAllocatedForLabel, CoreSchema.Int32);

			// skip final 16 bytes
			writer.WriteCode("00000000 00000000 00000000 00000000");
			writer.WriteCode("00000000 00000000 00000000 00000000");
			writer.WriteCode("00000000 00000000 00000000 00000000");
			writer.WriteCode("00000000 00000000 00000000 00000000");

			// Label
			Code packedLabel = PiedPiper.EncodeModel<FlexModel>(model.Label, CoreSchema.FlexModel);
			if (packedLabel.Length > (model.BytesAllocatedForLabel * 8))
			{
				throw new InvalidOperationException(
					$"Packed label length {packedLabel.Length:N0} ({(packedLabel.Length + 7) / 8:N0} bytes) " +
					$"exceeds {model.BytesAllocatedForLabel:N0} bytes " +
					"allocated for label."
				);
			}
			PiedPiper.PackModel(writer, model.Label, CoreSchema.FlexModel);

			// Label padding
			int paddedBitLength = (model.BytesAllocatedForLabel * 8) - (packedLabel.Length);
			if(paddedBitLength > 0) 
				writer.WriteCode(new Code(paddedBitLength));
		}

		public override TapeHeader UnpackModel(CodeReader reader)
		{
			// "TAPE" (for correctness)
			string tape = PiedPiper.UnpackModel<string>(reader, CoreSchema.TextAscii);
			// crap, we need a way of packing text without length prefix!!
			if (tape != "TAPE")
				throw new InvalidOperationException("Incorrect tape header. Missing 'TAPE' constant.");

			TapeHeader model = new TapeHeader();

			int bytesToSkip = 0;
			++bytesToSkip;  // "|"

			// "v1" (for show)
			bytesToSkip += 2;
			++bytesToSkip;  // "|"

			// series name (for show)
			bytesToSkip += 70;
			++bytesToSkip;  // "|"

			// series number (for show)
			bytesToSkip += 5;
			++bytesToSkip;  // "|"

			// GUID (for show)
			bytesToSkip += 16;

			// End-of-file (for show)
			++bytesToSkip;

			reader.Read(bytesToSkip * 8);

			// Version (actual)
			int version = PiedPiper.UnpackModel<int>(reader, CoreSchema.Int32);
			model.Version = version;

			// Bytes Allocated For Label (actual)
			int bytesAllocatedForLabel = PiedPiper.UnpackModel<int>(reader, CoreSchema.Int32);
			model.BytesAllocatedForLabel = bytesAllocatedForLabel;

			// skip final 16 bytes
			reader.Read(16 * 8);

			// Label
			model.Label = PiedPiper.UnpackModel<FlexModel>(reader, CoreSchema.FlexModel);

			return model;
		}
		#endregion

		#region private functions
		private static string Make5DigitNumber(int number)
		{
			Debug.Assert(number >= 0);

			if (number > 99999)
				return "XXXXX";

			return number.ToString("D5");
		}

		private static string MakeFixedSizeAscii(string? str, int desiredLength)
		{
			Debug.Assert(desiredLength > 0);

			if (string.IsNullOrEmpty(str))
				return new string(' ', desiredLength);

			StringBuilder builder = new StringBuilder();
			for(int i = 0; i < desiredLength; ++i)
			{
				char c = ' ';
				if(i < str.Length) 
					c = str[i];

				if (c >= 32 && c < 128)
					builder.Append(c);
				else
					builder.Append('?');
			}

			return builder.ToString();
		}
		#endregion
	}
}
