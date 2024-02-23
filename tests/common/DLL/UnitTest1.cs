using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dead;

namespace DLL
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void LoadDLLTest()
		{
			Assert.IsTrue(Test());
			bool Test()
			{
				try
				{
					var dll = new Dead.DLL("ClassLibrary1.dll");
					dll.LoadClass("ClassLibrary1.Class1");
					return true;
				}
				catch (System.Exception)
				{
					return false;
				}
			}
		}
		[TestMethod]
		public void CallPublicMethodTest()
		{
			Assert.IsTrue(Test());
			bool Test()
			{
				try
				{
					var dll = new Dead.DLL("ClassLibrary1.dll");
					dll.LoadClass("ClassLibrary1.Class1");
					bool r = (bool)dll.CallMethod("PublicBoolMethod", new object[]{ 1, 2 });
					System.Console.WriteLine("return value={0}", r);
					return r;
				}
				catch (System.Exception)
				{
					return false;
				}
			}
		}
		[TestMethod]
		public void CallPrivateMethodTest()
		{
			Assert.ThrowsException<System.NullReferenceException>(Test);
			void Test()
			{
				var dll = new Dead.DLL("ClassLibrary1.dll");
				dll.LoadClass("ClassLibrary1.Class1");
				// 外部から呼べないので、NullReferenceException がスローされれば OK
				dll.CallMethod("PrivateBoolMethod", new object[]{ "test" });
			}
		}
		[TestMethod]
		public void CallPublicStaticTest()
		{
			Assert.IsTrue(Test());
			bool Test()
			{
				try
				{
					var dll = new Dead.DLL("ClassLibrary1.dll");
					dll.LoadClass("ClassLibrary1.Class1");
					dll.CallMethod("PublicStaticMethod");
					return true;
				}
				catch (System.Exception)
				{
					return false;
				}
			}
		}
	}
}
