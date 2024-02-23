
using System.Threading.Tasks;

class Program {
	static Dead.Tree.Reader? tree = null;
	static async Task Main() {
		tree = new Dead.Tree.Reader(@"..\..\..\test.txt", OnFinishedToRead);
		await Task.Run(tree.Start);
	}
	static void OnFinishedToRead() {
		Console.WriteLine("Finished to read");
		tree?.Node?.Dump();
	}
}
