using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dead;

namespace AlphaDecimal
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			int a = 10;
			string b = a.ToAlphabet();
			System.Console.WriteLine("{0}=\"{1}\"", a, b);
			Assert.IsTrue(b.ToLower() == "j");
			Assert.IsTrue(b.ToInt() == a);
			Assert.IsTrue(b.ToUint() == a);
			a = -1;
			b = a.ToAlphabet();
			System.Console.WriteLine("{0}=\"{1}\"", a, b);
			Assert.IsTrue(string.IsNullOrEmpty(b));
			Assert.IsTrue(b.ToInt() == 0);
			Assert.IsTrue(b.ToUint() == 0);
			a = 255;
			b = a.ToAlphabet();			
			System.Console.WriteLine("{0}=\"{1}\"", a, b);
			Assert.IsTrue(b.ToLower() == "iu");
			Assert.IsTrue(b.ToInt() == a);
			Assert.IsTrue(b.ToUint() == a);
		}
	}
}
