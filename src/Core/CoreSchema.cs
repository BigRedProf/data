namespace BigRedProf.Data.Core
{
	/// <summary>
	/// Contains the list of core schema identifiers.
	/// </summary>
	/// <remarks>
	/// To use the core pack rats, call <see cref="IPiedPiper.RegisterCorePackRats"/> after
	/// creating your main pied piper instance.
	/// </remarks>
	public static class CoreSchema
	{
		#region static fields
		/// <summary>
		/// The schema identifier for the <see cref="Bit"/> type.
		/// </summary>
		public const string Bit = "776985ea-f7a3-44d4-8132-ae2bfcfb8134";

		/// <summary>
		/// The schema identifier for the <see cref="Boolean"/> type.
		/// </summary>
		public const string Boolean = "ec22de73-2aee-4c15-ac7a-72a01adef828";

		/// <summary>
		/// The schema identifier for the <see cref="Byte"/> type.
		/// </summary>
		public const string Byte = "0715f4cb-541a-4969-be40-fedb5dd4f3be";

		/// <summary>
		/// The schema identifier for the <see cref="Char"/> type.
		/// </summary>
		public const string Char = "0958db49-3bed-41ed-9ae0-bec19f281400";

		/// <summary>
		/// The schema identifier for the <see cref="Code"/> type.
		/// </summary>
		public const string Code = "124c3883-2b87-4df2-b28d-70fd5c96d178";

		/// <summary>
		/// The schema identifier for the <see cref="DateTimeWithKind"/> type including the kind.
		/// </summary>
		/// <remarks>
		/// Note this schema packs values using 66 bits (64 for ticks and 2 for the kind).
		/// If you don't care about the kind, you can use the <see cref="DateTimeWithoutKind"/> schema
		/// instead and save 2 bits.
		/// </remarks>
		public const string DateTimeWithKind = "e231824c-affe-4369-8db8-d46242af5e97";

		/// <summary>
		/// The schema identifier for the <see cref="DateTimeWithKind"/> type without a kind.
		/// </summary>
		/// <remarks>
		/// By not including <see cref="DateTime.Kind"></see> in the schema, the kind is assumed to be
		/// <see cref="DateTime.Kind.Unspecified"/>. This is useful when you don't care about the
		/// kind property and want to save space. It saves 2 bits by reducing the packed size from 66
		/// bits to 64 bits
		/// </remarks>
		public const string DateTimeWithoutKind = "fd712fc5-d24b-44dc-820f-ee7bdbc683b7";

		/// <summary>
		/// The schema identifier for the <see cref="Decimal"/> type.
		/// </summary>
		public const string Decimal = "882e48d6-7a62-4051-a6df-14c9c59c8efd";

		/// <summary>
		/// The schema identifier for the <see cref="Double"/> type.
		/// </summary>
		public const string Double = "2482c3b5-43a1-4dd4-b9ba-214d20325bcc";

		/// <summary>
		/// The schema identifier for efficient 31-bit whole numbers.
		/// </summary>
		public const string EfficientWholeNumber31 = "303eedd7-2f7f-4d6a-9e94-de3523322b33";

		/// <summary>
		/// The schema identifier for efficient 63-bit whole numbers.
		/// </summary>
		public const string EfficientWholeNumber63 = "6a29f5c6-fb2b-4e5c-8d77-89371cd4a1f0";

		/// <summary>
		/// The schema identifier for the <see cref="FlexModel"/> type.
		/// </summary>
		public const string FlexModel = "94059e98-41f8-4adc-b29e-1c0630645809";
		
		/// <summary>
		/// The schema identifier for the <see cref="Guid"/> type.
		/// </summary>
		public const string Guid = "5e0d9656-ea94-4664-8803-408edc337a28";

		/// <summary>
		/// The schema identifier for the <see cref="Identifier"/> type.
		/// </summary>
		public const string Identifier = "058ee310-339b-43ed-8cac-88d889c36d14";

		/// <summary>
		/// The schema identifier for the <see cref="Int16"/> type.
		/// </summary>
		public const string Int16 = "30d87859-ba7b-455e-a5d1-cdce8c973069";

		/// <summary>
		/// The schema identifier for the <see cref="Int32"/> type.
		/// </summary>
		public const string Int32 = "ffdb57bb-be6b-4cc3-a0f6-48596c8a8b2b";

		/// <summary>
		/// The schema identifier for the <see cref="Int64"/> type.
		/// </summary>
		public const string Int64 = "05d2e9ce-16cb-4fa0-832a-f933d9bfe1d6";

		/// <summary>
		/// The schema identifier for the <see cref="ModelWithSchema"/> type.
		/// </summary>
		public const string ModelWithSchema = "adce8290-2438-420d-9658-49296cf456f3";

		/// <summary>
		/// The schema identifier for the <see cref="ModelWithSchemaAndLength"/> type.
		/// </summary>
		public const string ModelWithSchemaAndLength = "c02015c9-fe26-4c65-93bc-2c67da6fb3c3";

		/// <summary>
		/// The schema identifier for packs.
		/// </summary>
		public const string Pack = "98bdc72c-e554-4ec8-bd45-dafe012da009";

		/// <summary>
		/// The schema identifier for packages.
		/// </summary>
		public const string Package = "4778505c-65d9-4fec-8556-e1babc1e26de";

		/// <summary>
		/// The schema identifier for the <see cref="SByte"/> type.
		/// </summary>
		public const string SByte = "a46367ea-408e-4dee-91f5-4966743827eb";

		/// <summary>
		/// The schema identifier for the <see cref="Single"/> type.
		/// </summary>
		public const string Single = "4687a488-4e7e-4cd9-8d7d-58d40b1a2d14";

		/// <summary>
		/// The schema identifier for ASCII-encoded text.
		/// </summary>
		public const string TextAscii = "af494088-5e94-44bf-81d9-e0bcd62df092";

		/// <summary>
		/// The schema identifier for UTF-8-encoded text.
		/// </summary>
		public const string TextUtf8 = "9cdf52b4-4c47-4b6d-bc17-34f33312b7a7";

		/// <summary>
		/// The schema identifier for UTF-16-encoded text.
		/// </summary>
		public const string TextUtf16 = "cf1820e1-86b7-4e1c-909e-c9b874996e58";

		/// <summary>
		/// The schema identifier for UTF-32-encoded text.
		/// </summary>
		public const string TextUtf32 = "720b6a0a-48e9-426b-b84a-e03c43c6d209";

		/// <summary>
		/// The schema identifier for the <see cref="TimeSpan"/> type.
		/// </summary>
		public const string TimeSpan = "4719be8d-8f7f-4765-9bcb-c21cee89a400";

		/// <summary>
		/// The schema identifier for the <see cref="UInt16"/> type.
		/// </summary>
		public const string UInt16 = "f585c49a-f432-4ebb-bfda-45519462a9c0";

		/// <summary>
		/// The schema identifier for the <see cref="UInt32"/> type.
		/// </summary>
		public const string UInt32 = "d6453df6-551c-45de-898a-6c1139c93931";

		/// <summary>
		/// The schema identifier for the <see cref="UInt64"/> type.
		/// </summary>
		public const string UInt64 = "40374605-edeb-4da2-ae04-b768c913aa9a";

		/// <summary>
		/// The schema identifier for unsigned LEB128 variable-length integers.
		/// </summary>
		public const string VarInt = "502e414e-6228-4b32-b3a2-6d48d6836d97";

		/// <summary>
		/// The schema identifier for 1-bit whole numbers.
		/// </summary>
		public const string WholeNumber1 = "2dc55385-2de4-4396-9366-ac230b4d029e";

		/// <summary>
		/// The schema identifier for 2-bit whole numbers.
		/// </summary>
		public const string WholeNumber2 = "1919552e-83d6-46eb-a1a9-b126919ab00c";

		/// <summary>
		/// The schema identifier for 3-bit whole numbers.
		/// </summary>
		public const string WholeNumber3 = "7f4c64d5-9b23-4bda-8247-33898a81c73a";

		/// <summary>
		/// The schema identifier for 4-bit whole numbers.
		/// </summary>
		public const string WholeNumber4 = "c2039551-cff2-4255-80b9-079dff17dcd7";

		/// <summary>
		/// The schema identifier for 5-bit whole numbers.
		/// </summary>
		public const string WholeNumber5 = "18896d8f-f5ba-4a12-974c-179fc43d281b";

		/// <summary>
		/// The schema identifier for 6-bit whole numbers.
		/// </summary>
		public const string WholeNumber6 = "a88e7217-b2b9-4281-8636-3d5ff0bfea75";

		/// <summary>
		/// The schema identifier for 7-bit whole numbers.
		/// </summary>
		public const string WholeNumber7 = "8100a1ff-4ff9-4454-9cbb-2a511ce0f17f";

		/// <summary>
		/// The schema identifier for 8-bit whole numbers.
		/// </summary>
		public const string WholeNumber8 = "a0db2f62-1e4b-4a54-a263-64aa7fd3c4a5";

		/// <summary>
		/// The schema identifier for 9-bit whole numbers.
		/// </summary>
		public const string WholeNumber9 = "6bb20b6e-244f-47f5-adc9-bdc06b7a1b46";

		/// <summary>
		/// The schema identifier for 10-bit whole numbers.
		/// </summary>
		public const string WholeNumber10 = "7f1886a2-8f00-4763-b205-8bfd5cae97ab";

		/// <summary>
		/// The schema identifier for 11-bit whole numbers.
		/// </summary>
		public const string WholeNumber11 = "83713cc2-6198-41d6-98e2-8d5dba256e21";

		/// <summary>
		/// The schema identifier for 12-bit whole numbers.
		/// </summary>
		public const string WholeNumber12 = "f848e4e6-180a-437d-8bda-21602db75ea9";

		/// <summary>
		/// The schema identifier for 13-bit whole numbers.
		/// </summary>
		public const string WholeNumber13 = "ddc2e826-0174-4f06-bc30-d4a67ba17216";

		/// <summary>
		/// The schema identifier for 14-bit whole numbers.
		/// </summary>
		public const string WholeNumber14 = "45762744-9523-4759-9a6d-feaa8880012a";

		/// <summary>
		/// The schema identifier for 15-bit whole numbers.
		/// </summary>
		public const string WholeNumber15 = "a43d4e40-702c-406f-bbc6-d33215b3e013";

		/// <summary>
		/// The schema identifier for 16-bit whole numbers.
		/// </summary>
		public const string WholeNumber16 = "b15c43c2-1291-4737-b39b-1c02aadb6237";

		/// <summary>
		/// The schema identifier for 17-bit whole numbers.
		/// </summary>
		public const string WholeNumber17 = "aa70b0e3-1a8d-48c7-8fa3-faeb5169f194";

		/// <summary>
		/// The schema identifier for 18-bit whole numbers.
		/// </summary>
		public const string WholeNumber18 = "0aeb0767-4dbb-4579-8a7d-2fe791ed45ad";

		/// <summary>
		/// The schema identifier for 19-bit whole numbers.
		/// </summary>
		public const string WholeNumber19 = "9d34a6ee-f3a3-42e9-a40d-f5c8f3f654e2";

		/// <summary>
		/// The schema identifier for 20-bit whole numbers.
		/// </summary>
		public const string WholeNumber20 = "855a9dd7-fb9f-40a4-a5b0-6bfa9656f38e";

		/// <summary>
		/// The schema identifier for 21-bit whole numbers.
		/// </summary>
		public const string WholeNumber21 = "1db69ca5-1e0a-4482-b405-a4dbbd03b106";

		/// <summary>
		/// The schema identifier for 22-bit whole numbers.
		/// </summary>
		public const string WholeNumber22 = "2f72e117-c4f9-4ff3-9449-aee05cd13145";

		/// <summary>
		/// The schema identifier for 23-bit whole numbers.
		/// </summary>
		public const string WholeNumber23 = "97621ea5-0fc3-4b9b-8f4c-c7ba3aeb3d07";

		/// <summary>
		/// The schema identifier for 24-bit whole numbers.
		/// </summary>
		public const string WholeNumber24 = "1603e323-d0f9-4ddb-978b-1110b685c2dd";

		/// <summary>
		/// The schema identifier for 25-bit whole numbers.
		/// </summary>
		public const string WholeNumber25 = "d4a6e735-39ec-42ca-af9f-424af2c16019";

		/// <summary>
		/// The schema identifier for 26-bit whole numbers.
		/// </summary>
		public const string WholeNumber26 = "a0f25905-07d4-47fb-936d-0ba4d78f1530";

		/// <summary>
		/// The schema identifier for 27-bit whole numbers.
		/// </summary>
		public const string WholeNumber27 = "eed5d451-95b6-4ca9-a45e-dfbfef34dc83";

		/// <summary>
		/// The schema identifier for 28-bit whole numbers.
		/// </summary>
		public const string WholeNumber28 = "43b5165e-b14b-43b6-9951-e19129704de8";

		/// <summary>
		/// The schema identifier for 29-bit whole numbers.
		/// </summary>
		public const string WholeNumber29 = "8895a9cb-1950-4d42-95ca-f1e80650f822";

		/// <summary>
		/// The schema identifier for 30-bit whole numbers.
		/// </summary>
		public const string WholeNumber30 = "49484d60-0bcf-4996-9181-4cbd9d0de0a8";

		/// <summary>
		/// The schema identifier for 31-bit whole numbers.
		/// </summary>
		public const string WholeNumber31 = "6ab0203c-ed3b-4cdf-8577-d7b7305ee677";
		#endregion
	}
}
