using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dead;

namespace Constraint
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void LockTest()
		{
            _ = Assert.ThrowsException<Constraint<int>.LockedValueException>(test);
			void test()
			{
				var c = new Constraint<int>() { Value = 10 };
				c.Lock();
				Assert.IsTrue(c.IsLocked);
				c.Value = 2;
			}
		}
	}
}
