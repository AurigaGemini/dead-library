/*!
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。
 *         新しい文法に変更しないでください。
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Dead.Settings {
///////////////////////////////////////////////////////////////////////////////

/*!
	設定ファイルを読み込み、キーと値をペアとするデータリストに変換する。

	フォーマット
		キー:値[改行]

	設定ファイルは文字コード UTF-8 で記述する。
	前後のスペース、タブなどの空白は全て無視される。
	; か # か ' か - か ! か * か // 以降は改行までをコメントとみなす。
	キーもしくは値の途中に空白があっても（恐らく）正しく読み取れる。
	キーもしくは値の途中に半角の : を含めると正しく読み取れない。
*/
public class Reader {
	public class FailedToReadFile : System.Exception {}
	public class FileNotFound : System.Exception {}

	//////////////////////////////////////

	readonly Dictionary<string, string> _datas = new Dictionary<string, string>();
	readonly Action _on_finished_callback;

	//////////////////////////////////////

	public Dictionary<string, string> Datas {
		get {
			var d = new Dictionary<string, string>();
			Dictionary<string, string>.KeyCollection keys = this._datas.Keys;
			foreach (string k in keys) { d.Add(k, this._datas[k]); }

			return d;
		}
	}

	public List<string> Keys {
		get {
			var l = new List<string>();
			Dictionary<string, string>.KeyCollection keys = this._datas.Keys;
			foreach (string k in keys) { l.Add(k); }

			return l;
		}
	}

	public string FilePath { get; protected set; }

	//////////////////////////////////////

	public string GetValue(string key) {
		if (!this._datas.ContainsKey(key)) { return ""; }

		return this._datas[key];
	}

	public Reader(string settings_file_path, Action on_finished_callback) {
		Debug.Assert(on_finished_callback != null);
		if (!File.Exists(settings_file_path)) { throw new FileNotFound(); }

		this.FilePath = settings_file_path;
		this._on_finished_callback = on_finished_callback;
	}

	public void Start() {
		var task = System.Threading.Tasks.Task.Run(this.Read);
	}

	public void Reload(Action on_finished_callback) {
		Debug.Assert(on_finished_callback != null);
		var task =  System.Threading.Tasks.Task.Run(this.Read);
	}

	//////////////////////////////////////

	async Task Read() {
		try {
			using (var r = new StreamReader(this.FilePath, System.Text.Encoding.GetEncoding("utf-8"))) {
				this._datas.Clear();
				while (r.Peek() >= 0) {
					string s = await r.ReadLineAsync();
					if (string.IsNullOrEmpty(s)) { continue; }

					s = s.Trim();
					if (s.Length <= 0) { continue; }	//空行なので飛ばし

					string[] cols = s.Split(':');
					for (int i = 0; i < cols.Length; i++) { cols[i] = cols[i].Trim(); }

					if (cols[0][0] == ';' || cols[0][0] == '#' || cols[0][0] == '\'' || cols[0][0] == '-' || cols[0][0] == '!' || cols[0][0] == '*') { continue; }	//コメント行
					else if (cols[0].Length > 1 && cols[0].Substring(0, 2) == "//") { continue; }	//コメント行
					else {
						string k = cols[0];
						string v = cols[1];
						if (cols.Length > 2) {
							for (int i = 2; i < cols.Length; i++) { v += ":" + cols[2]; }
						}

						this._datas.Add(k, v);
					}
				}

				r.Close();
				this._on_finished_callback.Invoke();
			}
		}
		catch (System.Exception e) {
			Console.WriteLine("ファイルの読み込みに失敗\n{0}", e.Message);
			throw new FailedToReadFile();
		}
	}
}

///////////////////////////////////////////////////////////////////////////////
}
