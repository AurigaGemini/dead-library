/*!
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。
 *         新しい文法に変更しないでください。
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dead.Tsv {
///////////////////////////////////////////////////////////////////////////////

public class Data {
	public class RowNumberNotMatch : System.Exception {};
	public class ColumnNumberNotMatch : System.Exception {};

	/*!
	 * @param col_size_max 全ての行の桁数をこの値に固定する場合は正の値を指定、固定しない場合は 0 以下の値を指定する。
	 *                     IDを除いた桁数を指定する。１行に１０個の項目がある場合は９を指定。
	 * @param row_size_max 行数をこの値に固定する場合は正の値を指定、固定しない場合は 0 以下の値を指定する。
	 */
	public Data(uint col_size_max = 0, uint row_size_max = 0) {
		this._col_size_max = col_size_max;
		this._row_size_max = row_size_max;
	}

	~Data() {
		this.datas.Clear();
		this.ids.Clear();
		this.Count = 0;
	}

	public bool IsEmpty => this.Count <= 0;

	public uint Count { get; protected set; }

	public bool FixColumnSize => this._col_size_max > 0;

	public uint MaxRowNumber  => this._row_size_max > 0 ? this._row_size_max : 0;

	//////////////////////////////////////

	public IEnumerator<string> GetEnumerator() {
		if (this.datas == null) { yield break; }

		foreach (string id in this.ids) {
			yield return id;
		}
	}

	public List<string> this[string id] => this.GetValues(id);

	//////////////////////////////////////

	public bool HasID(string id) {
		return this.ids?.Contains(id) ?? false;
	}

	public void Clear() {
		this.datas.Clear();
		this.ids.Clear();
		this.Count = 0;
	}

	/// id と一致する一行分の値を返す
	public List<string> GetValues(string id) {
		if (this.Count <= 0) {
			return new List<string>();
		}

		if (!this.datas.ContainsKey(id)) {
			return new List<string>();
		}

		return this.datas[id].ConvertAll(x => (string)x.Clone()); // deep copy
	}

	/// 全ての ID を返す
	public List<string> GetIDs() { return this.ids; }

	/// 指定した値を持つ行の ID を全て返す
	public List<string> GetIDsFromValue(string value) {
		var id_list = new List<string>();
		Dictionary<string, List<string>>.KeyCollection keys = this.datas.Keys;

		foreach (string key in keys) {
			List<string> values = this.datas[key];
			foreach (string v in values) {
				if (v == value) {
					id_list.Add(key);
					break;
				}
			}
		}

		return id_list;
	}

	/*!
	 * @brief  １行分の値を追加する
	 * @param  id     = その行に割り当てる ID
	 * @param  values = その行に割り当てる値
	 * @return 成功時に true を返す。
	 *         id が未登録の場合 false を返す。
	 *         FixColumnSize == true なら values.Count が col_size_max と異なる場合 false を返す。
	 */
	public void AddRow(string id, List<string> values) {
		if (this.ids.Contains(id)) { return; } // 既に登録されている

		if (this._col_size_max > 0) { // 桁数を固定する設定
			if ((int)this._col_size_max != values.Count) { throw new ColumnNumberNotMatch(); } // 桁数が違う
		}

		if (this._row_size_max > 0) { // 行数を固定する設定
			if ((int)this._row_size_max <= this.Count) { throw new RowNumberNotMatch(); } // 指定行数を超える
		}

		this.datas.Add(id, values);
		this.ids.Add(id);
		this.Count++;
	}

	/*!
	 * @brief  指定位置の値を変更する
	 * @param  id            = 変更する行の ID
	 * @param  column_number = 変更する値の桁数(1以上)
	 * @param  value         = 変更する値
	 * @return 成功時に true を返す。
	 *         id が未登録の場合 false を返す。
	 *         FixColumnSize == true なら column_number が col_size_max より大きい場合 false を返す。
	 *         id がある行の桁数が column_number より小さい場合 false を返す。
	 */
	public bool Set(string id, uint column_number, string value) {
		if (this.ids.Contains(id)) { return false; } // 既に登録されている

		if (this._col_size_max > 0) { // 桁数を固定する設定
			if (this._col_size_max < column_number) { return false; } // 桁数が違う
		}

		if ((int)this.datas[id].Count <= column_number) { return false; } // 桁数が違う

		this.datas[id][(int)column_number] = value;
		return true;
	}

	/*!
	 * @brief  指定位置の値を返す
	 * @param  id            = 変更する行の ID
	 * @param  column_number = 変更する値の桁数(1以上)
	 * @return 指定位置の値を返す。
	 *         id が未登録の場合、空文字を返す。
	 *         FixColumnSize == true なら column_number が col_size_max より大きい場合、空文字を返す。
	 *         id がある行の桁数が column_number より小さい場合、空文字を返す。
	 */
	public string Get(string id, uint column_number) {
		if (this.ids.Contains(id)) { return ""; } // 既に登録されている

		if (this._col_size_max > 0) { // 桁数を固定する設定
			if (this._col_size_max < column_number) { return ""; } // 桁数が違う
		}

		if ((int)this.datas[id].Count <= column_number) { return ""; } // 桁数が違う

		return this.datas[id][(int)column_number];
	}

	//////////////////////////////////////

	readonly List<string> ids = new List<string>(); // 登録している id のリスト（順不同）
	readonly Dictionary</*id*/string, List<string>> datas = new Dictionary<string, List<string>>();

	readonly uint _col_size_max = 0;
	readonly uint _row_size_max = 0;
}

/*!
	tsv ファイルの読み込みクラス

	tsv ファイルのフォーマット@n
		value[1][1]\tvalue[1][2]\t...改行@n
		value[2][1]\tvalue[2][2]\t...改行

	tsv ファイルは文字コード UTF-8(BOMあり) で記述する。

	行前後のスペース、タブなどの空白は全て無視される。

	value にタブを含めることはできない。@n
	"value" ダブルクォーテーションで挟んでもタブと改行を含めることはできない。@n
	ダブルクォーテーションで挟んでも取り除かれない。

	行頭に ; か # か ' か - か ! か * か // がある場合、@n
	その行をコメント行とみなし、読み込みをスキップする。

	左端の桁(value[n][1])は ID 指定専用として扱われる。@n
	同じ ID を複数指定することはできない。@n
	ID は桁数にはカウントされず、桁値に含まれない。@n
	ID は別途取得する必要がある。→ GetIDs()@n
	ID は特定の行にアクセスする場合のキーになる。@n
	この設計のため、行数を指定することはできない。

	このクラスは、読み込みに失敗した場合、例外を投げる。@n
	このクラスで定義されている例外の他に、.Net の StreamReader が投げる例外も投げられる。@n
	従って、このクラスを利用する際、例外を補足する処理が必要になる場合がある。
*/
public class Reader {
	public class RowNumberNotMatch : System.Exception {};
	public class ColumnNumberNotMatch : System.Exception {};
	public class FileNotFound : System.Exception {};
	public class FailedToReadFile : System.Exception {};
	public class DuplicateID : System.Exception {};

	//////////////////////////////////////

	Action _on_finished_read = null;

	//////////////////////////////////////

	public Data   Data       { get; protected set; }
	public string FilePath   { get; protected set; }
	public bool   IsCanceled { get; protected set; }	// 読み込みを中断したら true
	public bool   IsStarted  { get; protected set; }	// 読み込みを開始したら true
	public bool   IsFinished { get; protected set; }	// 読み込みが完了(成功)したら true
	public bool   HasError   { get; protected set; }	// 読み込みに失敗したら true

	public Reader(string settings_file_path, uint col_size_max = 0, uint row_size_max = 0) {
		Console.WriteLine("TSV Reader file_path={0} col_size_max={0} row_size_max={0}", settings_file_path, col_size_max, row_size_max);
		if (!File.Exists(settings_file_path)) { throw new FileNotFound(); }

		this.IsCanceled = false;
		this.IsStarted  = false;
		this.IsFinished = false;
		this.HasError   = false;
		this.FilePath   = settings_file_path;
		this.Data       = new Data(col_size_max, row_size_max);
		Debug.Assert(this.Data != null);
	}

	public void Start(Action on_finished_read) {
		Console.WriteLine("TSV Reader Start");
		this._on_finished_read = on_finished_read;
		this.IsStarted        = true;
		this.IsFinished       = false;
		this.HasError         = false;
		var task = Task.Run(this.Read);
	}

	public void Cancel() {
		this.IsCanceled = true;
	}

	//////////////////////////////////////

	async Task Read() {
		try {
			this.IsCanceled = false;
			using (var stream_reader = new StreamReader(this.FilePath, System.Text.Encoding.GetEncoding("utf-8"))) {
				this.Data?.Clear();
				while (stream_reader.Peek() >= 0 && !this.IsCanceled) {
					string s = await stream_reader.ReadLineAsync();
					if (string.IsNullOrEmpty(s)) { continue; }

					s = s.Trim();
					if (s.Length <= 0) { continue; }	//空行なので飛ばし

					string[] cols = s.Split('\t');
					if (cols == null || cols.Length == 0) { continue; }

					for (int i = 0; i < cols.Length; i++) { cols[i] = cols[i].Trim(); }

					char? head = cols[0][0];
					if (head == null) { continue; }

					string id = cols[0]; // 左端の桁値は ID として扱われる
					if (string.IsNullOrEmpty(id)) { continue; }

					if (this.Data?.HasID(id) ?? false) { throw new DuplicateID(); } // ID が既に登録されている

					if (head == ';' || head == '#' || head == '\'' || head == '-' || head == '!' || head == '*') { continue; }	//コメント行
					else if (id.Length > 1 && id.Substring(0, 2) == "//") { continue; }	//コメント行
					else {
						var values = new List<string>();
						for (int i = 1; i < cols.Length; i++) { values.Add(cols[i]); }

						this.Data?.AddRow(id, values);
					}
				}

				stream_reader.Close();
				this.IsFinished = true;
				this._on_finished_read?.Invoke();
			}
		}
		catch (System.Exception e) {
			this.Data     = null;
			this.HasError = true;
			     if (typeof(RowNumberNotMatch)    == e.GetType()) { throw new RowNumberNotMatch(); }
			else if (typeof(ColumnNumberNotMatch) == e.GetType()) { throw new ColumnNumberNotMatch(); }
			else if (typeof(DuplicateID)          == e.GetType()) { throw new DuplicateID(); }
			else                                                  { throw new FailedToReadFile(); }
		}
	}
}

///////////////////////////////////////////////////////////////////////////////
}