namespace BigRedProf.Data.Tape.Test
{
	public class RestorationWizardTests
	{
		#region unit tests
		[Trait("Region", "RestorationWizard functions")]
		[Fact]
		public void OpenExistingTapeSeries_ShouldThrow_WhenLibraryIsNull()
		{
			throw new NotImplementedException();
		}

		[Trait("Region", "RestorationWizard functions")]
		[Fact]
		public void OpenExistingTapeSeries_ShouldThrow_WhenSeriesIdIsEmpty()
		{
			throw new NotImplementedException();
		}

		[Trait("Region", "RestorationWizard functions")]
		[Fact]
		public void OpenExistingTapeSeries_ShouldThrow_WhenOffsetIsOutOfRange()
		{
			throw new NotImplementedException();
		}

		[Trait("Region", "RestorationWizard functions")]
		[Fact]
		public void OpenExistingTapeSeries_ShouldOpenAnExistingTapeSeries()
		{
			// TODO: Create a new tapes series.
			// Write 1,000,000,003 bits of data to the tapes series (enough to reach a 2nd tape).
			// Use RestorationWizard.OpenExistingTapeSeries to open the tape series at offset 500
			// and verify the Position is at 500.
			// Read some data and verify it matches what was written.
			// Use RestorationWizard.OpenExistingTapeSeries to open the tape series at offset 1,000,000,003
			// and verify the Position is at 1,000,000,003.
			// Read some data and verify it matches what was written.
			throw new NotImplementedException();
		}
		#endregion
	}
}
