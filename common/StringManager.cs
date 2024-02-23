/*!
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。
 *         新しい文法に変更しないでください。
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dead.String {
///////////////////////////////////////////////////////////////////////////////

/*!
	IDに紐づけた複数の文字列をキューで管理するためのクラス。@n
	チャットログのような登録順が重要なものが対象。@n

	@note データ構造@n
	ＩＤ１┬文字列１@n
	　　　├文字列２@n
	　　　├文字列３@n
	　　　└文字列ｎ@n
	ＩＤ２┬文字列１@n
	　　　├文字列２@n
	　　　├文字列３@n
	　　　└文字列ｎ@n
	…@n
	ＩＤｎ┬文字列１@n
	　　　…
*/
public class Queue {
	readonly Dictionary<ulong, Queue<string>> _msg = new Dictionary<ulong, Queue<string>>();

	public void Clear() {
		this._msg.Clear();
	}

	/*!
		idで登録している文字列の数を返す。
	*/
	public uint GetCount(ulong id) {
		if (!this._msg.ContainsKey(id)) { return 0; }

		return (uint)this._msg[id].Count;
	}

	/*!
		idで登録している文字列のキューを配列に変換して返す。
	*/
	public bool GetArray(ulong id, out string[] a) {
		a = new string[]{};
		Queue<string> q;

		if (!this._msg.TryGetValue(id, out q)) {
			return false;
		}

		a = q.ToArray();
		return true;
	}

	/*!
		idで登録している全てのメッセージを取得し、内部リストからidで登録している全てのメッセージを削除する。
	*/
	public bool PopAll(ulong id, ref Queue<string> q) {
		Queue<string> strings;

		if (!this._msg.TryGetValue(id, out strings)) {
			return false;
		}

		if (strings == null) { return false; }

		q = strings;
		this._msg[id] = new Queue<string>();
		return true;
	}

	/*!
		idで登録している先頭のメッセージを取得し、そのメッセージのみを削除する。
	*/
	public bool Pop(ulong id, out string msg) {
		msg = "";
		Queue<string> q;

		if (!this._msg.TryGetValue(id, out q)) {
			return false;
		}

		if (q == null || q.Count < 1) {
			return true;
		}

		msg = q.Dequeue();
		this._msg[id] = q;
		return true;
	}

	/*!
		idで内部リストにメッセージを登録する。@n
		内部リストに既にidが存在する場合は、idに紐付けられているメッセージキューの末尾に追加する。@n
		idが存在しない場合は、idに紐付いたメッセージキューを作成し、内部リストに追加する。
	*/
	public bool Add(ulong id, string msg) {
		if (this._msg.ContainsKey(id)) {
			//idが登録されている場合はメッセージを追加
			Queue<string> q;
			if (!this._msg.TryGetValue(id, out q)) {
				return false;
			}

			if (q == null) { return false; }

			q.Enqueue(msg);
			this._msg[id] = q;
		} else {
			//idが登録されていない場合は新規追加
			var q = new Queue<string>();
			q.Enqueue(msg);
			this._msg.Add(id, q);
		}

		return true;
	}

	/*!
		idに紐付けられたメッセージキューを内部リストから削除する。
	*/
	public bool Remove(ulong id) {
		return this._msg.Remove(id);
	}
}

/*!
 *	@brief 多言語管理用のストリングテーブル
 *	@note  原則、追加したものは削除しないことを想定。
 *	       どうしても必要なら Clear を使い、必要なものを Add し直す。
 */
public class Table {
	public enum Language {
		Japanese,
		USEnglish,
		//UKEnglish,
		//	必要に応じて追加する
	}

	public bool IsEmpty {
		get {
			Debug.Assert(this._table != null);
			return this._table.Count <= 0;
		}
	}

	public int Count {
		get {
			Debug.Assert(this._table != null);
			return this._table.Count;
		}
	}

	public bool HasID(string id) {
		Debug.Assert(this._table != null);
		return this._table.ContainsKey(id);
	}

	public bool HasLanguage(string id, Language language) {
		Debug.Assert(this._table != null);

		if (this._table.ContainsKey(id)) {
			Languages languages = this._table[id];
			return languages.IsExisted(language);
		}

		return false;
	}

	public bool Add(string id, string text, Language language) {
		Debug.Assert(this._table != null);

		if (this._table.ContainsKey(id)) {
			//	id を登録済み
			Languages languages = this._table[id];
			return languages.Add(language, text);
		}
		else {
			//	id は未登録
			var languages = new Languages();
			if (!languages.Add(language, text)) { return false; }

			this._table.Add(id, languages);
		}

		return true;
	}

	public void Clear() {
		Debug.Assert(this._table != null);

		Dictionary<string, Languages>.KeyCollection keys = this._table.Keys;
		foreach (string key in keys) {
			this._table[key].Clear();
		}

		this._table.Clear();
	}

	public string Get(string id, Language language) {
		Debug.Assert(this._table != null);

		if (!this._table.ContainsKey(id)) { return string.Empty; }

		return this._table[id].Get(language);
	}

	//////////////////////////////////////

	class Languages {
		public bool IsEmpty => this.Count <= 0;

		public uint Count => (uint)this._datas.Count;

		public bool Add(Language language, string text) {
			if (this.IsExisted(language)) { return false; }	// 既に登録している

			var data = new Tuple<Language, string>(language, text);
			this._datas.Add(data);
			return true;
		}

		public void Clear() {
			this._datas.Clear();
		}

		public string Get(Language language) {
			if (!this.IsExisted(language)) { return ""; } // language は未登録

			foreach (Tuple<Language, string> data in this._datas) {
				if (data.Item1 == language) { return data.Item2; }
			}

			return string.Empty;
		}

		public bool IsExisted(Language language) {
			foreach (Tuple<Language, string> data in this._datas) {
				if (data.Item1 == language) { return true; }
			}

			return false;
		}

		readonly List<Tuple<Language, string>> _datas = new List<Tuple<Language, string>>();
	}

	readonly Dictionary</*string_id*/string, Languages> _table = new Dictionary<string, Languages>();
}

///////////////////////////////////////////////////////////////////////////////
}
