using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data
{
	/// <summary>
	/// Contains a list of default schema identifiers. Additional schema identifiers can be added via
	/// extension properties.
	/// </summary>
	public static class SchemaId
	{
		#region static fields
		/// <summary>
		/// The schema identifier for the <see cref="Bit"/> type.
		/// </summary>
		public const string Bit = "776985EA-F7A3-44D4-8132-AE2BFCFB8134";

		/// <summary>
		/// The schema identifier for the <see cref="Boolean"/> type.
		/// </summary>
		public const string Boolean = "EC22DE73-2AEE-4C15-AC7A-72A01ADEF828";

		/// <summary>
		/// The schema identifier for the <see cref="Byte"/> type.
		/// </summary>
		public const string Byte = "0715F4CB-541A-4969-BE40-FEDB5DD4F3BE";

		/// <summary>
		/// The schema identifier for the <see cref="Char"/> type.
		/// </summary>
		public const string Char = "0958DB49-3BED-41ED-9AE0-BEC19F281400";

		/// <summary>
		/// The schema identifier for the <see cref="Code"/> type.
		/// </summary>
		public const string Code = "124C3883-2B87-4DF2-B28D-70FD5C96D178";

		/// <summary>
		/// The schema identifier for the <see cref="DateTime"/> type.
		/// </summary>
		public const string DateTime = "E231824C-AFFE-4369-8DB8-D46242AF5E97";

		/// <summary>
		/// The schema identifier for the <see cref="Decimal"/> type.
		/// </summary>
		public const string Decimal = "882E48D6-7A62-4051-A6DF-14C9C59C8EFD";

		/// <summary>
		/// The schema identifier for the <see cref="Double"/> type.
		/// </summary>
		public const string Double = "2482C3B5-43A1-4DD4-B9BA-214D20325BCC";

		/// <summary>
		/// The schema identifier for efficient 31-bit whole numbers.
		/// </summary>
		public const string EfficientWholeNumber31 = "303EEDD7-2F7F-4D6A-9E94-DE3523322B33";

		/// <summary>
		/// The schema identifier for efficient 63-bit whole numbers.
		/// </summary>
		public const string EfficientWholeNumber63 = "6A29F5C6-FB2B-4E5C-8D77-89371CD4A1F0";

		/// <summary>
		/// The schema identifier for the <see cref="Guid"/> type.
		/// </summary>
		public const string Guid = "5E0D9656-EA94-4664-8803-408EDC337A28";

		/// <summary>
		/// The schema identifier for the <see cref="Identifier"/> type.
		/// </summary>
		public const string Identifier = "058EE310-339B-43ED-8CAC-88D889C36D14";

		/// <summary>
		/// The schema identifier for the <see cref="Int16"/> type.
		/// </summary>
		public const string Int16 = "30D87859-BA7B-455E-A5D1-CDCE8C973069";

		/// <summary>
		/// The schema identifier for the <see cref="Int32"/> type.
		/// </summary>
		public const string Int32 = "FFDB57BB-BE6B-4CC3-A0F6-48596C8A8B2B";

		/// <summary>
		/// The schema identifier for the <see cref="Int64"/> type.
		/// </summary>
		public const string Int64 = "05D2E9CE-16CB-4FA0-832A-F933D9BFE1D6";

		/// <summary>
		/// The schema identifier for packs.
		/// </summary>
		public const string Pack = "98BDC72C-E554-4EC8-BD45-DAFE012DA009";

		/// <summary>
		/// The schema identifier for packages.
		/// </summary>
		public const string Package = "4778505C-65D9-4FEC-8556-E1BABC1E26DE";

		/// <summary>
		/// The schema identifier for the <see cref="SByte"/> type.
		/// </summary>
		public const string SByte = "A46367EA-408E-4DEE-91F5-4966743827EB";

		/// <summary>
		/// The schema identifier for the <see cref="Single"/> type.
		/// </summary>
		public const string Single = "4687A488-4E7E-4CD9-8D7D-58D40B1A2D14";

		/// <summary>
		/// The schema identifier for ASCII-encoded text.
		/// </summary>
		public const string TextAscii = "AF494088-5E94-44BF-81D9-E0BCD62DF092";

		/// <summary>
		/// The schema identifier for UTF-7-encoded text.
		/// </summary>
		public const string TextUtf7 = "4A7D3FE3-5945-4D25-AE6A-CE19F24E038C";

		/// <summary>
		/// The schema identifier for UTF-8-encoded text.
		/// </summary>
		public const string TextUtf8 = "9CDF52B4-4C47-4B6D-BC17-34F33312B7A7";

		/// <summary>
		/// The schema identifier for UTF-16-encoded text.
		/// </summary>
		public const string TextUtf16 = "CF1820E1-86B7-4E1C-909E-C9B874996E58";

		/// <summary>
		/// The schema identifier for UTF-32-encoded text.
		/// </summary>
		public const string TextUtf32 = "720B6A0A-48E9-426B-B84A-E03C43C6D209";

		/// <summary>
		/// The schema identifier for the <see cref="TimeSpan"/> type.
		/// </summary>
		public const string TimeSpan = "4719BE8D-8F7F-4765-9BCB-C21CEE89A400";

		/// <summary>
		/// The schema identifier for the <see cref="UInt16"/> type.
		/// </summary>
		public const string UInt16 = "F585C49A-F432-4EBB-BFDA-45519462A9C0";

		/// <summary>
		/// The schema identifier for the <see cref="UInt32"/> type.
		/// </summary>
		public const string UInt32 = "D6453DF6-551C-45DE-898A-6C1139C93931";

		/// <summary>
		/// The schema identifier for the <see cref="UInt64"/> type.
		/// </summary>
		public const string UInt64 = "40374605-EDEB-4DA2-AE04-B768C913AA9A";

		/// <summary>
		/// The schema identifier for 1-bit whole numbers.
		/// </summary>
		public const string WholeNumber1 = "2DC55385-2DE4-4396-9366-AC230B4D029E";

		/// <summary>
		/// The schema identifier for 2-bit whole numbers.
		/// </summary>
		public const string WholeNumber2 = "1919552E-83D6-46EB-A1A9-B126919AB00C";

		/// <summary>
		/// The schema identifier for 3-bit whole numbers.
		/// </summary>
		public const string WholeNumber3 = "7F4C64D5-9B23-4BDA-8247-33898A81C73A";

		/// <summary>
		/// The schema identifier for 4-bit whole numbers.
		/// </summary>
		public const string WholeNumber4 = "C2039551-CFF2-4255-80B9-079DFF17DCD7";

		/// <summary>
		/// The schema identifier for 5-bit whole numbers.
		/// </summary>
		public const string WholeNumber5 = "18896D8F-F5BA-4A12-974C-179FC43D281B";

		/// <summary>
		/// The schema identifier for 6-bit whole numbers.
		/// </summary>
		public const string WholeNumber6 = "A88E7217-B2B9-4281-8636-3D5FF0BFEA75";

		/// <summary>
		/// The schema identifier for 7-bit whole numbers.
		/// </summary>
		public const string WholeNumber7 = "8100A1FF-4FF9-4454-9CBB-2A511CE0F17F";

		/// <summary>
		/// The schema identifier for 8-bit whole numbers.
		/// </summary>
		public const string WholeNumber8 = "A0DB2F62-1E4B-4A54-A263-64AA7FD3C4A5";

		/// <summary>
		/// The schema identifier for 9-bit whole numbers.
		/// </summary>
		public const string WholeNumber9 = "6BB20B6E-244F-47F5-ADC9-BDC06B7A1B46";

		/// <summary>
		/// The schema identifier for 10-bit whole numbers.
		/// </summary>
		public const string WholeNumber10 = "7F1886A2-8F00-4763-B205-8BFD5CAE97AB";

		/// <summary>
		/// The schema identifier for 11-bit whole numbers.
		/// </summary>
		public const string WholeNumber11 = "83713CC2-6198-41D6-98E2-8D5DBA256E21";

		/// <summary>
		/// The schema identifier for 12-bit whole numbers.
		/// </summary>
		public const string WholeNumber12 = "F848E4E6-180A-437D-8BDA-21602DB75EA9";

		/// <summary>
		/// The schema identifier for 13-bit whole numbers.
		/// </summary>
		public const string WholeNumber13 = "DDC2E826-0174-4F06-BC30-D4A67BA17216";

		/// <summary>
		/// The schema identifier for 14-bit whole numbers.
		/// </summary>
		public const string WholeNumber14 = "45762744-9523-4759-9A6D-FEAA8880012A";

		/// <summary>
		/// The schema identifier for 15-bit whole numbers.
		/// </summary>
		public const string WholeNumber15 = "A43D4E40-702C-406F-BBC6-D33215B3E013";

		/// <summary>
		/// The schema identifier for 16-bit whole numbers.
		/// </summary>
		public const string WholeNumber16 = "B15C43C2-1291-4737-B39B-1C02AADB6237";

		/// <summary>
		/// The schema identifier for 17-bit whole numbers.
		/// </summary>
		public const string WholeNumber17 = "AA70B0E3-1A8D-48C7-8FA3-FAEB5169F194";

		/// <summary>
		/// The schema identifier for 18-bit whole numbers.
		/// </summary>
		public const string WholeNumber18 = "0AEB0767-4DBB-4579-8A7D-2FE791ED45AD";

		/// <summary>
		/// The schema identifier for 19-bit whole numbers.
		/// </summary>
		public const string WholeNumber19 = "9D34A6EE-F3A3-42E9-A40D-F5C8F3F654E2";

		/// <summary>
		/// The schema identifier for 20-bit whole numbers.
		/// </summary>
		public const string WholeNumber20 = "855A9DD7-FB9F-40A4-A5B0-6BFA9656F38E";

		/// <summary>
		/// The schema identifier for 21-bit whole numbers.
		/// </summary>
		public const string WholeNumber21 = "1DB69CA5-1E0A-4482-B405-A4DBBD03B106";

		/// <summary>
		/// The schema identifier for 22-bit whole numbers.
		/// </summary>
		public const string WholeNumber22 = "2F72E117-C4F9-4FF3-9449-AEE05CD13145";

		/// <summary>
		/// The schema identifier for 23-bit whole numbers.
		/// </summary>
		public const string WholeNumber23 = "97621EA5-0FC3-4B9B-8F4C-C7BA3AEB3D07";

		/// <summary>
		/// The schema identifier for 24-bit whole numbers.
		/// </summary>
		public const string WholeNumber24 = "1603E323-D0F9-4DDB-978B-1110B685C2DD";

		/// <summary>
		/// The schema identifier for 25-bit whole numbers.
		/// </summary>
		public const string WholeNumber25 = "D4A6E735-39EC-42CA-AF9F-424AF2C16019";

		/// <summary>
		/// The schema identifier for 26-bit whole numbers.
		/// </summary>
		public const string WholeNumber26 = "A0F25905-07D4-47FB-936D-0BA4D78F1530";

		/// <summary>
		/// The schema identifier for 27-bit whole numbers.
		/// </summary>
		public const string WholeNumber27 = "EED5D451-95B6-4CA9-A45E-DBFBEF34DC83";

		/// <summary>
		/// The schema identifier for 28-bit whole numbers.
		/// </summary>
		public const string WholeNumber28 = "43B5165E-B14B-43B6-9951-E19129704DE8";

		/// <summary>
		/// The schema identifier for 29-bit whole numbers.
		/// </summary>
		public const string WholeNumber29 = "8895a9cb-1950-4d42-95ca-f1e80650f822";

		/// <summary>
		/// The schema identifier for 30-bit whole numbers.
		/// </summary>
		public const string WholeNumber30 = "49484D60-0BCF-4996-9181-4CBD9D0DE0A8";

		/// <summary>
		/// The schema identifier for 31-bit whole numbers.
		/// </summary>
		public const string WholeNumber31 = "6AB0203C-ED3B-4CDF-8577-D7B7305EE677";
		#endregion
	}
}
