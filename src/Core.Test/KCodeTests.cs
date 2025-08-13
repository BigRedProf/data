using System;
using BigRedProf.Data.Core;
using Xunit;

namespace BigRedProf.Data.Test
{
    public class KCodeTests
    {
        [Fact]
        public void CanCreateKCode_AtMaxLength()
        {
            var kcode = new KCode(KCode.MaxLength);
            Assert.Equal(KCode.MaxLength, kcode.Length);
        }

        [Fact]
        public void CanCreateKCode_AtMinLength()
        {
            var kcode = new KCode(1);
            Assert.Equal(1, kcode.Length);
        }

        [Fact]
        public void Throws_WhenLengthExceedsMax()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new KCode(KCode.MaxLength + 1));
        }

        [Fact]
        public void Throws_WhenBitsExceedMax()
        {
            Bit[] bits = new Bit[KCode.MaxLength + 1];
            Assert.Throws<ArgumentOutOfRangeException>(() => new KCode(bits));
        }

        [Fact]
        public void Throws_WhenByteArrayLengthExceedsMax()
        {
            byte[] bytes = new byte[(KCode.MaxLength / 8) + 2];
            Assert.Throws<ArgumentOutOfRangeException>(() => new KCode(bytes, KCode.MaxLength + 1));
        }

        [Fact]
        public void KCode_BehavesLikeCode()
        {
            var code = new Code("101010");
            var kcode = new KCode("101010");
            Assert.Equal(code.Length, kcode.Length);
            Assert.Equal(code.ToString(), kcode.ToString());
            Assert.Equal(code, kcode);
            Assert.True(kcode == new KCode("101010"));
            Assert.False(kcode != new KCode("101010"));
        }

        [Fact]
        public void KCode_CastOperators_Work()
        {
            var kcode = new KCode("101010");
            Code code = kcode; // implicit
            Assert.Equal(kcode.ToString(), code.ToString());
            var kcode2 = (KCode)code; // explicit
            Assert.Equal(kcode.ToString(), kcode2.ToString());
        }

        [Fact]
        public void KCode_ConstructorVariants_Work()
        {
            var k1 = new KCode(4);
            Assert.Equal(4, k1.Length);
            var k2 = new KCode("1010");
            Assert.Equal("1010", k2.ToString().Replace(" ", ""));
            var bytes = new byte[] { 0b10101010 };
            var k3 = new KCode(bytes, 8);
            Assert.Equal(8, k3.Length);
            var k4 = new KCode(bytes);
            Assert.Equal(8, k4.Length);
            var k5 = new KCode(bytes, 8, 0b10101010);
            Assert.Equal(8, k5.Length);
        }
    }
}
