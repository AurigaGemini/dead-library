
class Test {
	static bool finished = false;
	static void Main() {
//		Dead.Tsv.Reader reader = new (@"..\..\..\test.tsv", 2, 3); // 通常動作確認用
//		Dead.Tsv.Reader reader = new (@"..\..\test.tsv", 2, 3); // パスが存在しないエラー確認用
//		Dead.Tsv.Reader reader = new (@"..\..\..\test.tsv", 1, 3); // 桁数不一致例外確認用
//		Dead.Tsv.Reader reader = new (@"..\..\..\test.tsv", 2, 2); // 行数超過例外確認用
		Dead.Tsv.Reader reader = new (@"..\..\..\test.tsv"); // 引数省略時の挙動確認用
		reader.Start(OnFinishedRead);

		finished = false;
		while (!finished) {
			Console.WriteLine("TSV Reading...");
			Thread.Sleep(1000);
		}
		Console.WriteLine("end");

		Dead.Tsv.Data? data = reader.Data;
		if (data != null) {
			foreach (var id in data) {
				Console.Write("ID={0} ", id);
				List<string> cols = data[id];
				foreach (var col in cols) {
					Console.Write("{0},", col);
				}
				Console.WriteLine();
			}
		}
	}
	static void OnFinishedRead() {
		Console.WriteLine("TSV Reading Finished");
		finished = true;
	}
}
