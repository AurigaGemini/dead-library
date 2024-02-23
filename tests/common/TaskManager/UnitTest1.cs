using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskManager
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void AddTask()
		{
			var property = new Dead.TaskManager.Property(0, "hoge");
			Assert.AreEqual<bool>(
				Dead.TaskManager.Add(
					property,
					()=>{ System.Console.WriteLine("Invoke Task category={0} name={1}", property.category, property.name); }
				),
				true
			);
			System.Console.WriteLine("task number={0} after Add", Dead.TaskManager.Count);
			Assert.AreEqual<int>(Dead.TaskManager.Count, 1);
			Dead.TaskManager.Update();
			System.Console.WriteLine("task number={0} after Update", Dead.TaskManager.Count);
			Assert.AreEqual<int>(Dead.TaskManager.Count, 1);
		}

		[TestMethod]
		public void RemoveTask()
		{
			var property = new Dead.TaskManager.Property(0, "hoge");
			Assert.AreEqual<bool>(
				Dead.TaskManager.Add(
					property,
					()=>{ System.Console.WriteLine("Invoke Task category={0} name={1}", property.category, property.name); }
				),
				true
			);
			System.Console.WriteLine("task number={0} after Add", Dead.TaskManager.Count);
			Dead.TaskManager.Update();
			System.Console.WriteLine("task number={0} after Update", Dead.TaskManager.Count);
			Assert.AreEqual<int>(Dead.TaskManager.Count, 1);
			Assert.AreEqual<bool>(
				Dead.TaskManager.Remove(property),
				true
			);
			System.Console.WriteLine("task number={0} after Remove", Dead.TaskManager.Count);
			Assert.AreEqual<int>(Dead.TaskManager.Count, 1);
			Dead.TaskManager.Update();
			System.Console.WriteLine("task number={0} after Remove and Update", Dead.TaskManager.Count);
			Assert.AreEqual<int>(Dead.TaskManager.Count, 0);
		}

		[TestMethod]
		public void ClearTask()
		{
			var property = new Dead.TaskManager.Property(0, "hoge");
			Assert.AreEqual<bool>(
				Dead.TaskManager.Add(
					property,
					()=>{ System.Console.WriteLine("Invoke Task category={0} name={1}", property.category, property.name); }
				),
				true
			);
			System.Console.WriteLine("task number={0} after Add", Dead.TaskManager.Count);
			Assert.AreEqual<int>(Dead.TaskManager.Count, 1);
			Dead.TaskManager.Update();
			System.Console.WriteLine("task number={0} after Update", Dead.TaskManager.Count);
			Assert.AreEqual<int>(Dead.TaskManager.Count, 1);
			Dead.TaskManager.Clear();
			System.Console.WriteLine("task number={0} after Clear", Dead.TaskManager.Count);
			Assert.AreEqual<int>(Dead.TaskManager.Count, 0);
		}
	}
}
