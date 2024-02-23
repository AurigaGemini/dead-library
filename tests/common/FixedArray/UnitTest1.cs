using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dead;

namespace FixedArray
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ReserveTest()
        {
            var a = new FixedArray<int>(10);
            int data = 111;
            uint index = a.Reserve(data);
            Assert.AreEqual<int>(a[index], data);
            data = 10;
            a[index] = data;
            Assert.AreEqual<int>(a[index], data);
        }

        [TestMethod]
        public void CountTest()
		{
            var a = new FixedArray<int>(10);
            a.Reserve(999);
            uint count = 1;
            Assert.AreEqual<uint>(a.Count, count);
		}

        [TestMethod]
        public void CapacityTest()
		{
            uint capacity = 10;
            var a = new FixedArray<int>(capacity);
            a.Reserve(999);
            Assert.AreEqual<uint>(a.Capacity, capacity);
		}
        
        [TestMethod]
        public void ExceptionTest1()
		{
            Assert.ThrowsException<FixedArray<int>.InvalidElement>(test1);
            Assert.ThrowsException<FixedArray<int>.InvalidElement>(test2);
            void test1()
            {
                var a = new FixedArray<int>(10);
                int b = a[0];
            }
            void test2()
            {
                var a = new FixedArray<int>(10);
                a[0] = 2;
            }
		}

        [TestMethod]
        public void ExceptionTest2()
		{
            Assert.ThrowsException<System.IndexOutOfRangeException>(test1);
            Assert.ThrowsException<System.IndexOutOfRangeException>(test2);
            void test1()
			{
                var a = new FixedArray<int>(0);
                int b = a[0];
			}
            void test2()
			{
                var a = new FixedArray<int>(0);
                a[0] = -5;
			}
        }
    }
}
