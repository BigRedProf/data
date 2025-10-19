using BigRedProf.Data.Core;
using BigRedProf.Data.Tape._TestHelpers;

namespace BigRedProf.Data.Tape.Test
{
	public class BackupWizardTests
	{
		#region constructors
		public BackupWizardTests()
		{
		}
		#endregion

		#region unit tests
		[Trait("Region", "Librarian methods")]
		public void MainScenario_ShouldWork()
		{
			TapeProvider tapeProvider = TapeProviderHelper.CreateMemoryTapeProvider();

			// TODO: implement a classic backup case
		}
		#endregion
	}
}
