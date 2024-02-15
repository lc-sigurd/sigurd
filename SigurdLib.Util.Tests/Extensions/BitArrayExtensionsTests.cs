using System;
using System.Collections;
using SigurdLib.Util.Extensions;

namespace SigurdLib.Common.Tests.Extensions;

public class BitArrayExtensionsTests
{
    //private static BitArray CreateBitArray()

    public class NextClearBitIndexTests
    {
        private void AssertNextClearBitIndexEquals(BitArray array, int fromIndex, int expectedIndex)
        {
            Assert.Equal(expectedIndex, array.NextClearBitIndex(fromIndex));
        }

        [Fact]
        public void TestNegativeArgument()
        {
            Assert.Throws<ArgumentException>(Main);

            void Main()
            {
                new BitArray(10, true).NextClearBitIndex(-1);
            }
        }

        [Fact]
        public void TestAllFalse()
        {
            AssertNextClearBitIndexEquals(new BitArray(32, false), 0, 0);
        }

        [Fact]
        public void TestAllFalseFromIndex()
        {
            AssertNextClearBitIndexEquals(new BitArray(32, false), 5, 5);
        }

        [Fact]
        public void TestAllFalseMultiWord()
        {
            AssertNextClearBitIndexEquals(new BitArray(96, false), 0, 0);
        }

        [Fact]
        public void TestAllTrue()
        {
            AssertNextClearBitIndexEquals(new BitArray(32, true), 0, 32);
        }

        [Fact]
        public void TestAllTrueMultiWord()
        {
            AssertNextClearBitIndexEquals(new BitArray(96, true), 0, 96);
        }

        [Fact]
        public void TestSparse()
        {
            int[] sparse = [0b00000000_00000000_00000000_01101111];
            AssertNextClearBitIndexEquals(new BitArray(sparse), 0, 4);
        }

        [Fact]
        public void TestSparseFromIndex()
        {
            int[] sparse = [0b00000000_00000000_00000000_01101111];
            AssertNextClearBitIndexEquals(new BitArray(sparse), 5, 7);
        }

        [Fact]
        public void TestSparseMultiWord()
        {
            int[] sparse = [-1, -1, 0b00000000_00000000_00000000_01101111];
            AssertNextClearBitIndexEquals(new BitArray(sparse), 64, 68);
        }

        [Fact]
        public void TestSparseMultiWordFromIndex()
        {
            int[] sparse = [-1, -1, 0b00000000_00000000_00000000_01101111];
            AssertNextClearBitIndexEquals(new BitArray(sparse), 69, 71);
        }
    }

    public class NextSetBitIndexTests
    {
        private void AssertNextSetBitIndexEquals(BitArray array, int fromIndex, int expectedIndex)
        {
            Assert.Equal(expectedIndex, array.NextSetBitIndex(fromIndex));
        }

        [Fact]
        public void TestNegativeArgument()
        {
            Assert.Throws<ArgumentException>(Main);

            void Main()
            {
                new BitArray(10, true).NextSetBitIndex(-1);
            }
        }

        [Fact]
        public void TestAllFalse()
        {
            AssertNextSetBitIndexEquals(new BitArray(32, false), 0, -1);
        }

        [Fact]
        public void TestAllFalseMultiWord()
        {
            AssertNextSetBitIndexEquals(new BitArray(96, false), 0, -1);
        }

        [Fact]
        public void TestAllTrue()
        {
            AssertNextSetBitIndexEquals(new BitArray(32, true), 0, 0);
        }

        [Fact]
        public void TestAllTrueFromIndex()
        {
            AssertNextSetBitIndexEquals(new BitArray(32, true), 5, 5);
        }

        [Fact]
        public void TestAllTrueMultiWord()
        {
            AssertNextSetBitIndexEquals(new BitArray(96, true), 0, 0);
        }

        [Fact]
        public void TestSparse()
        {
            int[] sparse = [0b00000000_01000000_10000000_00000000];
            AssertNextSetBitIndexEquals(new BitArray(sparse), 0, 15);
        }

        [Fact]
        public void TestSparseFromIndex()
        {
            int[] sparse = [0b00000000_01000000_10000000_00000000];
            AssertNextSetBitIndexEquals(new BitArray(sparse), 16, 22);
        }

        [Fact]
        public void TestSparseMultiWord()
        {
            int[] sparse = [0, 0, 0b00000000_01000000_10000000_00000000];
            AssertNextSetBitIndexEquals(new BitArray(sparse), 0, 79);
        }

        [Fact]
        public void TestSparseMultiWordFromIndex()
        {
            int[] sparse = [0, 0, 0b00000000_01000000_10000000_00000000];
            AssertNextSetBitIndexEquals(new BitArray(sparse), 80, 86);
        }
    }
}
