using BigRedProf.Data.Core;
using System;
using Xunit;
using Xunit.Abstractions;

namespace BigRedProf.Data.Core.Test
{
	public class ProbeTests
	{
		private readonly ITestOutputHelper _output;
		public ProbeTests(ITestOutputHelper output) { _output = output; }

		[Fact]
		public void Probe()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();

			foreach (string s in new[] { "", "a" })
			{
				try
				{
					Code c = piedPiper.PackModel<string>(s, CoreSchema.TextUtf8);
					string back = piedPiper.UnpackModel<string>(c, CoreSchema.TextUtf8);
					_output.WriteLine($"text \"{s}\" -> {c.Length} bits -> \"{back}\"");
				}
				catch (Exception ex) { _output.WriteLine($"text \"{s}\" FAILED: {ex.Message}"); }
			}

			try { Code c = ""; _output.WriteLine("empty string to Code: ok"); }
			catch (Exception ex) { _output.WriteLine($"empty string to Code FAILED: {ex.Message}"); }

			// A trait whose answer is an empty string, through the flex datum.
			piedPiper.DefineTrait(new Trait("11111111-0000-0000-0000-000000000001", CoreSchema.TextUtf8));
			try
			{
				FlexDatum fd = new FlexDatumBuilder(piedPiper)
					.AddTrait("11111111-0000-0000-0000-000000000001", "")
					.Build();
				Code c = piedPiper.PackModel<FlexDatum>(fd, CoreSchema.FlexDatum);
				FlexDatum back = piedPiper.UnpackModel<FlexDatum>(c, CoreSchema.FlexDatum);
				_output.WriteLine($"empty-string trait ok, value=\"{back.GetTrait<string>("11111111-0000-0000-0000-000000000001", piedPiper)}\"");
			}
			catch (Exception ex) { _output.WriteLine($"empty-string trait FAILED: {ex.GetType().Name}: {ex.Message}"); }
		}
	}
}
