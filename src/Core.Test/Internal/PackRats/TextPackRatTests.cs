using BigRedProf.Data.Core.Internal.PackRats;
using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.Text;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class TextPackRatTests
	{
		#region test data
		public static readonly Encoding[] SupportedEncodings = new Encoding[]
		{
			Encoding.ASCII,
			Encoding.UTF8,
			Encoding.Unicode, // UTF-16
			Encoding.UTF32
		};

		public static TheoryData<Encoding> GetEncodings()
		{
			var data = new TheoryData<Encoding>();
			foreach (var encoding in SupportedEncodings)
				data.Add(encoding);
			return data;
		}
		#endregion

		#region PackRat methods
		[Theory]
		[Trait("Region", "PackRat methods")]
		[MemberData(nameof(GetEncodings))]
		public void PackModel_ShouldThrowWhenWriterIsNull(Encoding encoding)
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			TextPackRat packRat = new TextPackRat(piedPiper, encoding);
			string model = string.Empty;

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRat.PackModel(null, model);
				}
			);
		}

		[Theory]
		[Trait("Region", "PackRat methods")]
		[MemberData(nameof(GetEncodings))]
		public void PackModel_ShouldWork(Encoding encoding)
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			PackRat<string> packRat = new TextPackRat(piedPiper, encoding);
			PackRatTestHelper.TestPackModel<string>(packRat, string.Empty, "1000");
		}

		[Theory]
		[Trait("Region", "PackRat methods")]
		[MemberData(nameof(GetEncodings))]
		public void UnpackModel_ShouldThrowWhenReaderIsNull(Encoding encoding)
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			TextPackRat packRat = new TextPackRat(piedPiper, encoding);

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRat.UnpackModel(null);
				}
			);
		}

		[Theory]
		[Trait("Region", "PackRat methods")]
		[MemberData(nameof(GetEncodings))]
		public void UnpackModel_ShouldWorkForTrue(Encoding encoding)
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			PackRat<string> packRat = new TextPackRat(piedPiper, encoding);
			PackRatTestHelper.TestUnpackModel<string>(packRat, "1000", string.Empty);
		}

		#endregion
	}
}
