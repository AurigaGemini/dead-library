using Dead.Task;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {
	[TestClass]
	public class UnitTest1 {
		struct Args {
			public int   code;
			public ulong id;
		}

		class TestChip : Dead.Task.IChip<Args> {
			public TestChip() {
				this.Data = new Args();
			}
			public int Run(object args) {
				var actualy_args = (Args)args;
				System.Console.WriteLine("id={0} code={1}", actualy_args.id, actualy_args.code++);
#pragma warning disable
				System.Threading.Tasks.Task.Delay(500);
				System.Console.WriteLine("id={0} code={1}", actualy_args.id, actualy_args.code++);
				System.Threading.Tasks.Task.Delay(500);
				System.Console.WriteLine("id={0} code={1}", actualy_args.id, actualy_args.code++);
				System.Threading.Tasks.Task.Delay(500);
				System.Console.WriteLine("id={0} code={1}", actualy_args.id, actualy_args.code);
#pragma warning restore
				return actualy_args.code;
			}
			public Args Data { get; set; }
		}

		Dead.Task.Rack<Args> CreateRackAndSetBoards(byte max_number, out System.Collections.Generic.List<ulong> id_list) {
			id_list = new System.Collections.Generic.List<ulong>();

			var rack = new Dead.Task.Rack<Args>(max_number);

			for (int i = 0; i < max_number; i++) {
				var   board    = new Dead.Task.Board<Args>(new TestChip());
				var   args     = new Args() { code = i * 10, id = Dead.IncrementalIDGenerator.Invalid_ID };
				ulong board_id = rack.Insert(board);

				Assert.IsFalse(Dead.Task.Rack<Args>.IsErrorID(board_id));

				args.id = board_id;
				id_list.Add(board_id);

				Assert.IsTrue(rack.PowerOn(board_id, args));

				System.Console.WriteLine("\tUniqueID={0} State={1} ExitCode={2}", board.UniqueID, board.State, board.ExitCode);
			}

			return rack;
		}

		bool SetBoards(Dead.Task.Rack<Args> rack, System.Collections.Generic.List<ulong> id_list) {
			foreach (ulong id in id_list) {
				var args = new Args() { code = (int)id * 10, id = Dead.IncrementalIDGenerator.Invalid_ID };

				Dead.Task.Board<Args> board = rack.GetBoard(id);
				if (board == null) { return false; }

				if (!rack.PowerOn(id, args)) {
					System.Console.WriteLine("Failed to rack.PowerOn({0}, args=[code={1},id={2}])", id, args.code, args.id);
					return false;
				}
			}

			return true;
		}

		bool SetBoardsDebugState(Dead.Task.Rack<Args> rack, Dead.Task.Board<Args>.Status status, System.Collections.Generic.List<ulong> id_list) {
			foreach (ulong id in id_list) {
				var args = new Args() { code = (int)id * 10, id = Dead.IncrementalIDGenerator.Invalid_ID };

				Dead.Task.Board<Args> board = rack.GetBoard(id);
				//System.Console.WriteLine("board={0}", board == null ? "null" : board.ToString());
				if (board == null) { return false; }

				//System.Console.WriteLine("board.State={0}", board.State);
				board.DebugForceState(status);
				//System.Console.WriteLine("board.State={0}", board.State);
			}

			return true;
		}

		void WaitForFinished(Dead.Task.Rack<Args> rack) {
			while (!rack.IsAllCompleted) {

			#pragma warning disable IDE0058
				System.Threading.Tasks.Task.Delay(500);
			#pragma warning restore IDE0058

			}
		}

		[TestMethod]
		public void TestBoardPowerOnFail() {
			var board = new Dead.Task.Board<Args>(new TestChip());
			var args  = new Args() { code = 1, id = Dead.IncrementalIDGenerator.Invalid_ID };
			//	’I‚É“ü‚ê‚È‚¢‚Æ“dŒ¹‚ª“ü‚ç‚È‚¢‚Ì‚Å¸”s‚·‚ê‚Î OK
			Assert.IsFalse(board.PowerOn(args));
		}

		[TestMethod]
		public void TestInsert() {
			var board = new Dead.Task.Board<Args>(new TestChip());
			var args  = new Args() { code = 1, id = Dead.IncrementalIDGenerator.Invalid_ID };

			Dead.Task.Rack<Args> rack;
			ulong board_id;

			//	’I‚Ì‹ó‚«‚ª 0 ‚È‚Ì‚Å¸”s‚·‚é‚Í‚¸
			//	IsErrorID ‚ª true ‚É‚È‚ê‚Î OK
			rack = new Dead.Task.Rack<Args>(0);
			board_id = rack.Insert(board);
			Assert.IsTrue(Dead.Task.Rack<Args>.IsErrorID(board_id));

			//	’I‚Ì‹ó‚«‚ª‚ ‚é‚Ì‚Å¬Œ÷‚·‚é‚Í‚¸
			//	IsErrorID ‚ª false ‚É‚È‚ê‚Î OK
			rack = new Dead.Task.Rack<Args>(1);
			board_id = rack.Insert(board);
			Assert.IsFalse(Dead.Task.Rack<Args>.IsErrorID(board_id));
		}

		[TestMethod]
		public void TestPowerOnWithBoard() {
			var board = new Dead.Task.Board<Args>(new TestChip());
			var args  = new Args() { code = 1, id = Dead.IncrementalIDGenerator.Invalid_ID };

			var rack = new Dead.Task.Rack<Args>(1);
			ulong board_id = rack.Insert(board);
			Assert.IsFalse(Dead.Task.Rack<Args>.IsErrorID(board_id));

			//	ƒ{[ƒh‚©‚ç’¼Ú“dŒ¹‚ğ“ü‚ê‚é
			//	’I‚Éû”[‚µ‚½‚Ì‚Å“dŒ¹‚ª“ü‚é‚Í‚¸
			Assert.IsTrue(board.PowerOn(args));
		}

		[TestMethod]
		public void TestPowerOnWithRack() {
			var board = new Dead.Task.Board<Args>(new TestChip());
			var args  = new Args() { code = 1, id = Dead.IncrementalIDGenerator.Invalid_ID };

			var rack = new Dead.Task.Rack<Args>(1);
			ulong board_id = rack.Insert(board);
			Assert.IsFalse(Dead.Task.Rack<Args>.IsErrorID(board_id));

			//	’I‚©‚çƒ{[ƒh‚É“dŒ¹‚ğ“ü‚ê‚é
			//	’I‚Éû”[‚µ‚½‚Ì‚Å“dŒ¹‚ª“ü‚é‚Í‚¸
			Assert.IsTrue(rack.PowerOn(board_id, args));
		}

		[TestMethod]
		public void TestMultiBoards() {
			System.Collections.Generic.List<ulong> id_list;
			Dead.Task.Rack<Args> rack = this.CreateRackAndSetBoards(3, out id_list);

			this.WaitForFinished(rack);

			for (int i = 0; i < id_list.Count; i++) {
				Assert.IsTrue(rack.GetExitCode(id_list[i]) == i * 10 + 3);
			}
		}

		[TestMethod]
		public void TestEject() {
			System.Collections.Generic.List<ulong> id_list;
			Dead.Task.Rack<Args> rack = this.CreateRackAndSetBoards(3, out id_list);

			foreach (ulong id in id_list) {
				Dead.Task.Board<Args> board = rack.Eject(id);
				Assert.IsTrue(board != null);
				//System.Console.WriteLine("UniqueID={0} State={1} ExitCode={2}", board.UniqueID, board.State, board.ExitCode);
			}
		}

		[TestMethod]
		public void TestDestroy() {
			System.Collections.Generic.List<ulong> id_list;
			Dead.Task.Rack<Args> rack = this.CreateRackAndSetBoards(3, out id_list);

			rack.Destroy();

			Assert.IsTrue(rack.Count == 0);
		}

		[TestMethod]
		public void TestPowerOnAgain() {
			System.Collections.Generic.List<ulong> id_list;
			Dead.Task.Rack<Args> rack = this.CreateRackAndSetBoards(3, out id_list);

			this.WaitForFinished(rack);

			{
				ulong board_id = id_list[0];
   				var args = new Args() { code = 999, id = Dead.IncrementalIDGenerator.Invalid_ID };
				Assert.IsTrue(rack.PowerOn(board_id, args));
			}

			this.WaitForFinished(rack);
		}

		[TestMethod]
		public void TestPowerOffAll_DuringPowerOn() {
			System.Collections.Generic.List<ulong> id_list;
			byte board_number = 3;

			Dead.Task.Rack<Args> rack = this.CreateRackAndSetBoards(board_number, out id_list);

			//	Status ‚ªˆá‚¤‚Ì‚Å¸”s‚·‚é
			Assert.IsFalse(rack.PowerOffAll() == board_number);

			Assert.IsTrue(this.SetBoardsDebugState(rack, Board<Args>.Status.PowerOn, id_list));

			//	Status ‚ª“¯‚¶‚È‚Ì‚Å¬Œ÷‚·‚é
			Assert.IsTrue(rack.PowerOffAll() == board_number);
		}

		[TestMethod]
		public void TestPowerOffAll_AfterCompleted() {
			var  id_list      = new System.Collections.Generic.List<ulong>();
			byte board_number = 3;
			var  rack         = new Dead.Task.Rack<Args>(board_number);

			for (int i = 0; i < board_number; i++) {
				var   board    = new Dead.Task.Board<Args>(new TestChip());
				var   args     = new Args() { code = i * 10, id = Dead.IncrementalIDGenerator.Invalid_ID };
				ulong board_id = rack.Insert(board);

				Assert.IsFalse(Dead.Task.Rack<Args>.IsErrorID(board_id));

				args.id = board_id;
				id_list.Add(board_id);

				Assert.IsTrue(rack.PowerOn(board_id, args));
			}

			this.WaitForFinished(rack);

			//	ƒ^ƒXƒN‚ªŠù‚ÉI—¹‚µ‚Ä‚¢‚é‚Ì‚ÅAPowerOff ‚Å‚«‚éƒ{[ƒh‚Í‚È‚¢
			Assert.IsTrue(rack.PowerOffAll() == 0);
		}
	}
}
