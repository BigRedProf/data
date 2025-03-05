using System;

namespace BigRedProf.Data.Core.Internal.PackRats
{
    internal class WholeNumberPackRat : PackRat<int>
    {
		#region fields
		private readonly int _bitLength;
        private readonly int _maxValue;
		#endregion

		#region constructors
		public WholeNumberPackRat(IPiedPiper piedPiper, int bitLength)
            : base(piedPiper)
        {
            if (bitLength <= 0 || bitLength > 31)
                throw new ArgumentOutOfRangeException(nameof(bitLength), "Bit length must be between 0 and 31.");

            _bitLength = bitLength;
			_maxValue = (1 << _bitLength) - 1;
		}
        #endregion

        #region PackRat methods
        public override void PackModel(CodeWriter writer, int model)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (model < 0 || model > _maxValue)
                throw new ArgumentOutOfRangeException(nameof(model), $"Model value must be between 0 and {(1 << _bitLength) - 1} for {_bitLength}-bit whole number.");

            byte[] bytes = BitConverter.GetBytes(model);
            Code code = new Code(bytes, _bitLength);
            writer.WriteCode(code);
        }

        public override int UnpackModel(CodeReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Code code = reader.Read(_bitLength);
            if (code.ByteArray == null || code.ByteArray.Length * 8 < _bitLength)
                throw new InvalidOperationException("Invalid code length for whole number.");

            // ensure it's 4 bytes to satisfy BitConverter
            byte[] byteArray;
            if(code.ByteArray.Length == 4)
            {
                byteArray = code.ByteArray;
			}
            else
            {
                byteArray = new byte[4];
                Array.Copy(code.ByteArray, byteArray, code.ByteArray.Length);
            }

            int model = BitConverter.ToInt32(byteArray, 0);
            return model;
        }
        #endregion
    }
}
