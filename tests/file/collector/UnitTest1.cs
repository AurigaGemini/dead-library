using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace FileCollector
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestEmptyPaths()
        {
			var fc = new Dead.File.Collector();
			var paths = new List<string>();
			bool is_succeeded = false;

			try {
				Task t = fc.Start(paths);
				is_succeeded = true;
			}
			catch (System.Exception e) {
				if (e.GetType() != typeof(System.ArgumentNullException)) {
					//	ArgumentNullException 以外は想定外なので失敗させる
					Assert.Fail();
				}

				System.Console.WriteLine(e.ToString());
			}

			//	paths が空なので ArgumentNullException がスローされ、is_succeeded == false になる必要がある
			Assert.IsFalse(is_succeeded);
		}

		[TestMethod]
		public void TestOnePath()
		{
			var paths = new List<string>() {
				@"C:\Program Files\Internet Explorer"
			};
			var fc = new Dead.File.Collector();
			Task t = fc.Start(paths);
			t.Wait();
			Assert.IsTrue(t.IsCompleted && fc.IsCompleted);

			ImmutableList<string> result = fc.Result;
			System.Console.WriteLine("result.Count={0}", result.Count);
			foreach (string path in result) {
				System.Console.WriteLine(path);
			}
		}

		[TestMethod]
		public void TestMultiPath() {
			var paths = new List<string>() {
				@"C:\Program Files\Internet Explorer",
				@"C:\Program Files\Microsoft OneDrive",
				@"C:\Program Files\Microsoft Visual Studio"
			};
			var fc = new Dead.File.Collector();
			Task t = fc.Start(paths);
			t.Wait();
			Assert.IsTrue(t.IsCompleted && fc.IsCompleted);

			ImmutableList<string> result = fc.Result;
			System.Console.WriteLine("result.Count={0}", result.Count);
			foreach (string path in result) {
				System.Console.WriteLine(path);
			}
		}

		[TestMethod]
		public void TestMultiPathWithFilter() {
			var paths = new List<string>() {
				@"C:\Program Files\Internet Explorer",
				@"C:\Program Files\Microsoft OneDrive",
				@"C:\Program Files\Microsoft Visual Studio"
			};
			var fc = new Dead.File.Collector();
			Task t = fc.Start(paths, "dll");
			t.Wait();
			Assert.IsTrue(t.IsCompleted && fc.IsCompleted);

			ImmutableList<string> result = fc.Result;
			System.Console.WriteLine("result.Count={0}", result.Count);
			foreach (string path in result) {
				System.Console.WriteLine(path);
			}
		}

		[TestMethod]
		public void TestOnePathWithFilterOnFinished() {
			var paths = new List<string>() {
				@"C:\Program Files\Internet Explorer",
			};
			var fc = new Dead.File.Collector();

			bool is_finished = false;
			Task t = fc.Start(paths, "dll", (Dead.File.Collector caller) => {
				System.Console.WriteLine("Called OnFinished");
				Assert.IsTrue(caller.IsCompleted);
				is_finished = true;
			});

			t.Wait();

			int elapsed = 0;
			while (!is_finished || elapsed > 30000) {
				System.Threading.Thread.Sleep(1);
				elapsed++;
			}

			Assert.IsTrue(is_finished);
			System.Console.WriteLine("Finished");
		}

		[TestMethod]
		public void TestCancelOnFinished() {
			var paths = new List<string>() {
			@"C:\Program Files\Internet Explorer"
		};
			var fc = new Dead.File.Collector();

			bool is_finished = false;
			Task t = fc.Start(paths, "dll", (Dead.File.Collector caller) => {
				System.Console.WriteLine("Called OnFinished");
				Assert.IsFalse(caller.IsCompleted);
				Assert.IsFalse(caller.HasError);
				Assert.IsTrue(caller.IsCanceled);
				is_finished = true;
			});

			fc.Cancel();

			Assert.IsTrue(is_finished);
			System.Console.WriteLine("Finished");
		}

		[TestMethod]
		public void TestErrorOnFinished() {
			//	C:\Windows 直下には管理者権限がないとアクセスできないフォルダがある
			//	このテストプログラムを管理者権限を持たずに実行した場合にアクセス拒否で例外が投げられる
			//	それを FileCollector 側でキャッチして HasError が true になることを想定している
			//	管理者権限を持ってこのプログラムを実行した場合、例外が発生せず、テストに失敗するので注意
			var paths = new List<string>() {
				@"C:\Windows"
			};
			var fc = new Dead.File.Collector();

			bool is_finished = false;
			Task t = fc.Start(paths, "dll", (Dead.File.Collector caller) => {
				System.Console.WriteLine("Called OnFinished");
				Assert.IsFalse(caller.IsCompleted);
				Assert.IsTrue(caller.HasError);
				Assert.IsFalse(caller.IsCanceled);
				is_finished = true;
			});

			t.Wait();

			Assert.IsTrue(is_finished);
			System.Console.WriteLine("Finished");
		}

	}
}
