using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class EfficientWholeNumber31PackRat : PackRat<int>
	{
		#region constants
		private const int MinLegalValue = 0;
		private const int MaxLegalValue = 0b01111111111111111111111111111111; // 2^31 - 1 = 2147483647
		private const int MinValueFor8BitPacking = 4;
		private const int MinValueFor16BitPacking = MinValueFor8BitPacking + 32;    // 4 + 2^5 = 36
		private const int MinValueFor24BitPacking = MinValueFor16BitPacking + 4096; // 36 + 2^12 = 4132
		private const int MinValueFor32BitPacking = MinValueFor24BitPacking + 1048576; // 4132 + 2^20 = 1052708
		#endregion

		#region constructors
		public EfficientWholeNumber31PackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, int model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			if(model < MinLegalValue)
			{
				throw new ArgumentOutOfRangeException(
					nameof(model),
					$"Model must be at least {MinLegalValue}."
				);
			}

			if (model > MaxLegalValue)
			{
				throw new ArgumentOutOfRangeException(
					nameof(model),
					$"Model must be at most {MaxLegalValue}."
				);
			}

			if (model >= MinValueFor32BitPacking)
			{
				// huge numbers
				//		in the range [1052708-2147483647]
				//		0 ==> 32-bits total (1 marker, 31 data)
				//		NOTE: This does leave 1052707 codes unused.
				writer.WriteCode("0");
				WriteData(writer, 31, model);
				return;
			}

			if (model < MinValueFor8BitPacking)
			{
				// tiny numbers 
				//		in the range [0-3]
				//		10 ==> 4-bits total (2 marker, 2 data)
				writer.WriteCode("10");
				WriteData(writer, 2, model);
				return;
			}

			if (model < MinValueFor16BitPacking)
			{
				// small numbers
				//		in the range [4-35]
				//		110 ==> 8-bits total (3 marker, 5 data)
				writer.WriteCode("110");
				model -= MinValueFor8BitPacking;
				WriteData(writer, 5, model);
				return;
			}

			if (model < MinValueFor24BitPacking)
			{
				// medium numbers
				//		in the range [36-4131]
				//		1110 ==> 16-bits total (4 marker, 12 data) 
				writer.WriteCode("1110");
				model -= MinValueFor16BitPacking;
				WriteData(writer, 12, model);
				return;
			}

			Debug.Assert(model < MinValueFor32BitPacking);
			// large numbers
			//		in the range [4132-1052707]
			//		1111 ==> 24-bits total (4 marker, 20 data)
			writer.WriteCode("1111");
			model -= MinValueFor24BitPacking;
			WriteData(writer, 20, model);
		}

		public override int UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			int model;

			Bit bit = reader.Read(1)[0];
			if (bit == 0)
			{
				// huge numbers
				//		in the range [1052708-2147483647]
				//		0 ==> 32-bits total (1 marker, 31 data)
				//		NOTE: This does leave 1052707 codes unused.
				//			And we will throw if it's one of those unused codes.
				model = ReadData(reader, 31);
				if (model < MinValueFor32BitPacking)
					throw new InvalidOperationException("Invalid code.");
				return model;
			}

			bit = reader.Read(1)[0];
			if (bit == 0)
			{
				// tiny numbers 
				//		in the range [0-3]
				//		10 ==> 4-bits total (2 marker, 2 data)
				model = ReadData(reader, 2);
				return model;
			}

			bit = reader.Read(1)[0];
			if (bit == 0)
			{
				// small numbers
				//		in the range [4-35]
				//		110 ==> 8-bits total (3 marker, 5 data)
				model = ReadData(reader, 5);
				model += MinValueFor8BitPacking;
				return model;
			}

			bit = reader.Read(1)[0];
			if (bit == 0)
			{
				// medium numbers
				//		in the range [36-4131]
				//		1110 ==> 16-bits total (4 marker, 12 data) 
				model = ReadData(reader, 12);
				model += MinValueFor16BitPacking;
				return model;
			}

			// large numbers are packed into 24 bits
			//		in the range [4132-1052707]
			//		1111 ==> 24-bits total (4 marker, 20 data)
			model = ReadData(reader, 20);
			model += MinValueFor24BitPacking;
			return model;
		}
		#endregion

		#region private methods
		private void WriteData(CodeWriter writer, int bitCount, int model)
		{
			for (int i = 0; i < bitCount; ++i)
			{
				Bit bit = (model & 1);
				writer.WriteCode(new Code(new Bit[] { bit }));
				model >>= 1;
			}
			Debug.Assert(model == 0);
		}

		private int ReadData(CodeReader reader, int bitCount)
		{
			int model = 0;
			int mask = 1;
			for (int i = 0; i < bitCount; ++i)
			{
				if (reader.Read(1) == "1")
					model |= mask;

				unchecked
				{
					mask <<= 1;
				}
			}

			return model;
		}
		#endregion
	}

}

