using System;
using Dead.Task;

namespace Test {
	class Program {
		class Args {
			public ulong id = 0;
			public int code = 0;
			public Func<Board<Args>.Status, int> onFinished;
		}
		class Chip : IChip<Args> {
#pragma warning disable
			public int Run(object args) {
				this.Data = (Args)args;
				this.Data.code++;
				System.Console.WriteLine("id ={0} code={1}", this.Data.id, this.Data.code);
				System.Threading.Tasks.Task.Delay(500);
				this.Data.code++;
				System.Console.WriteLine("id ={0} code={1}", this.Data.id, this.Data.code);
				System.Threading.Tasks.Task.Delay(500);
				this.Data.code++;
				System.Console.WriteLine("id ={0} code={1}", this.Data.id, this.Data.code);
				System.Threading.Tasks.Task.Delay(500);
				if (this.Data.onFinished != null) {
					this.Data.onFinished.Invoke(Board<Args>.Status.Completed);
				}
				return this.Data.code;
			}
#pragma warning restore
			public Args Data { get; set; } = new Args();
		}
		static void Test(Rack<Args> rack, byte board_number) {
			for (byte i = 0; i < board_number; i++) {
				var args = new Args() { id = 0, code = i * 10, onFinished = Program.OnChipFinished };
				var board = new Board<Args>(new Chip());
				ulong id = rack.Insert(board);
				args.id = id;
				if (!rack.PowerOn(id, args)) { throw new System.InvalidOperationException(); }
			}
		}
		static void Main() {
			byte board_number = 5;
			var rack = new Rack<Args>(board_number);
			Program.Test(rack, board_number);
			while (!rack.IsAllCompleted) {
				System.Threading.Tasks.Task.Delay(100);
			}
		}
		static int OnChipFinished(Board<Args>.Status result) {
			System.Console.WriteLine("result={0}", result);
			return 0;
		}
	}
}
