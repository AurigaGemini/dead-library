/*!
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。
 *         新しい文法に変更しないでください。
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

#pragma warning disable IDE0161 // 範囲指定されたファイルが設定された namespace に変換
namespace Dead.Tree {
#pragma warning restore IDE0161 // 範囲指定されたファイルが設定された namespace に変換
///////////////////////////////////////////////////////////////////////////////

	/*!
		独自のデータ形式で書かれたテキストファイルを読み込み、解析して、
		ツリー構造のデータとして保持するクラス。

		ルートを複数持つことはできない。
		複数のルートが必要な場合、ルート直下のノードを親として扱うことで代用可能。

		Graphical User Interface の構造を設定するためのデータ形式として設計したが
		ツリー構造として扱えるデータであれば何に使っても良い。@n
		XMLの代用として使ったり、コンパイラやインタプリタを自作するときに必要になる
		構文解析木を定義するデータとしても使えそう。

		記述例
		@usage
			オプションウィンドウ {@n
			　width=25%, height=25%,@n
			　下地 { x=0, y=0, z=0, width=100%, height=100%, file=option_back.png, type=image }@n
			　タイトルバー { x=10, y=5, z=1, width=90%, height=10%, file=option_title_bar.png, type=titlebar }@n
			　閉じるボタン { type=button, help=閉じる }@n
			　ディスプレイ {}@n
			　オーディオ {}@n
			　コントロール {}@n
			　その他 {}@n
			}@n

		一行コメント@n
			; # ' // --@n
			上記の記号から改行までをコメントとみなす
	*/
	public class Node {

	public struct Child {
		/*!
		 *	@attention
		 *	Tree クラスでデータを読み込んだ後、すぐに別の形式のデータに変換して
		 *	インスタンスを破棄する使い方を想定している。
		 *	そのような使い方であれば、生存期間が短く、どこで値が変更されたのか？
		 *	を追跡しやすいので、保守容易性が向上する。
		 *	このような想定した使い方をしていれば、以下は public で問題ない。
		 *	従って、インスタンスを長期間保持するような想定外の使い方はしないこと。
		 */
#pragma warning disable S1104	// Fields should not have public accessibility
		public string name;
		public string value;	//children が null なら、この変数に値が入る。children が null でないなら、この変数は null
		public List<Child> children;
#pragma warning restore S1104	// Fields should not have public accessibility

		public Child GetCopy() {	//全てのメンバを値コピーした完全に独立したコピーを取得
			var c = new Child() {
				name  = this.name,
				value = this.value,
			};

			if (this.children == null) {
#pragma warning disable CS8625 // null リテラルを null 非許容参照型に変換できません。
					c.children = null;
#pragma warning restore CS8625 // null リテラルを null 非許容参照型に変換できません。
				} else {
				c.children = new List<Child>();

				foreach (Child child in this.children) {
					c.children.Add(child.GetCopy());
				}
			}

			return c;
		}
	}

	public string      ParentName { get; protected set; }
	public List<Child> Children   { get; protected set; }

	public Node(string parent_name, List<Child> children) {
		if (children == null) {
			string msg = "argument children is null";
			throw new System.ArgumentNullException(msg);
		}

		if (string.IsNullOrEmpty(parent_name)) {
			string msg = "argument parent_name is null or empty";
			throw new System.ArgumentNullException(msg);
		}

		this.ParentName = parent_name;
		this.Children   = new List<Child>();

		foreach (Child child in children) {
			this.Children.Add(child.GetCopy());
		}
	}

	public void Dump() {
		Console.WriteLine("ParentName={0} ChildCount={1}{{", this.ParentName, this.Children.Count);

		foreach (Child c in this.Children) { this.DumpChild(c, 1); }

		Console.WriteLine("}");
	}

	////////////////////////////////////////////////////////////////////////////

	void DumpChild(Child child, int nest) {
		if (child.children != null) {
			if (string.IsNullOrEmpty(child.name)) { throw new System.InvalidOperationException(); }

			for (int i = 0; i < nest; i++) { System.Console.Write("\t"); }

			Console.WriteLine("{0}:Count={1}{{", child.name, child.children.Count);

			foreach (Child c in child.children) {
				this.DumpChild(c, nest + 1);
			}

			for (int i = 0; i < nest; i++) { System.Console.Write("\t"); }

			Console.WriteLine("}");
		} else {
			for (int i = 0; i < nest; i++) { System.Console.Write("\t"); }

			Console.WriteLine("{0}={1}", child.name, child.value);
		}
	}
}

////////////////////////////////////////////////////////////////////////////////

public class Reader {
	public Dead.Tree.Node Node { get; protected set; }

	public string FilePath { get; protected set; }

	readonly Action on_finished_callback;

#pragma warning disable CS8618 // .Net Standard 2.0 仕様に合わせているため、この警告は無視して良い。
	public Reader(string path, Action on_finished_callback) {
		this.FilePath             = path;
		this.on_finished_callback = on_finished_callback;
	}
#pragma warning restore CS8618

	public void Start() {
		string s = this.ReadFromFile(this.FilePath);
		if (string.IsNullOrEmpty(s)) { throw new System.ArgumentNullException("Failed to ReadFromFile(" + this.FilePath + ")"); }

		int pos = 0;
		string parent_name = "";

		var children = new List<Dead.Tree.Node.Child>();
		Debug.Assert(children != null);

		if (pos < s.Length) {
			this.ReadSpaceAndComment(s, ref pos);

			if (this.IsMatchedName(s, ref pos, ref parent_name)) {
				if (s[pos] == '{') {
					pos++;
					if (this.ReadChildren(s, ref pos, ref children)) {
						this.Node = new Dead.Tree.Node(parent_name, children);
					} else {
						Dead.String.Analyzer.GetColAndRow(s, pos, ref this.col, ref this.row);
						this.error = -1;
					}
				} else {
					Dead.String.Analyzer.GetColAndRow(s, pos, ref this.col, ref this.row);
					this.error = -2;
				}
			} else {
				Dead.String.Analyzer.GetColAndRow(s, pos, ref this.col, ref this.row);
				this.error = -3;
			}
		}

		this.on_finished_callback.Invoke();
	}

	public void GetLastError(out int error_code, out int error_col, out int error_row) {
		if (this.error < 0) {
			error_code = this.error;
			error_col  = this.col;
			error_row  = this.row;
		} else {
			error_code = 0;
			error_col = error_row = -1;
		}
	}

	////////////////////////////////////////////////////////////////////////////

	int col = 0;
	int row = 0;
	int error = 0;

	string ReadFromFile(string path) {
		string s = "";

		using (var r = new System.IO.StreamReader(path)) {
			s += r.ReadToEnd();
			r.Close();
		}

		return s;
	}

	void ReadSpaceAndComment(string s, ref int pos) {
		char c;
		while (pos < s.Length) {
			c = s[pos];
			if (Dead.String.Analyzer.ReadSpaceOnce(s, ref pos)) { continue; }

			bool is_semicolon_comment     = c == ';';
			bool is_pound_comment         = c == '#';
			bool is_backslash_comment     = c == '\'';
			bool is_double_slash_comment  = pos < s.Length - 1 && c == '/' && s[pos + 1] == '/';
			bool is_double_hyphen_comment = pos < s.Length - 1 && c == '-' && s[pos + 1] == '-';
			bool is_comment = is_semicolon_comment || is_pound_comment || is_backslash_comment || is_double_slash_comment || is_double_hyphen_comment;

			if (is_comment) {
				Dead.String.Analyzer.ReadLineEnd(s, ref pos);
			} else {	//空白でも改行でもコメントでもない
				break;
			}
		}
	}

	bool IsMatchedName(string s, ref int pos, ref string name) {
		this.ReadSpaceAndComment(s, ref pos);
		char c = s[pos];
		var temp_string = new StringBuilder();
		name = "";

		if (c == ',') {	//あってもいいけど、良くはない
			pos++;
			return false;
		}

		if (Dead.String.Utility.IsMark(c)) {	//ここにはないはず
			return false;
		}

		int i = pos;
		for (; i < s.Length; i++) {
			c = s[i];
			if (c != '_' && (Dead.String.Utility.IsSpace(c) || Dead.String.Utility.IsMark(c))) {
				for (; i < s.Length; i++) {
					c = s[i];

					if (Dead.String.Utility.IsSpace(c)) { continue; }	//空白は飛ばす

					if (c != '=' && c != '{') { _ = temp_string.Clear(); }	//=でも{でもなかったら不正なデータとみなす

					break;
				}

				break;
			} else {
				_ = temp_string.Append(c);
			}
		}

		name = temp_string.ToString();
		if (string.IsNullOrEmpty(name)) { return false; }

		pos = i;
		return true;
	}

	string ReadValue(string s, ref int pos) {
		this.ReadSpaceAndComment(s, ref pos);
		var temp_string = new StringBuilder();
		char c;
		int i = pos;
		for (; i < s.Length; i++) {
			c = s[i];
			if (c == ',') {
				i++;
				break;
			} else if (c == '}') {
				break;
			} else {
				_ = temp_string.Append(c);
			}
		}

		pos = i;
		return temp_string.ToString();
	}

	bool ReadChildren(string s, ref int pos, ref List<Dead.Tree.Node.Child> children) {
		char c;
		string n;

		while (pos < s.Length) {
			this.ReadSpaceAndComment(s, ref pos);

			n = "";
			c = s[pos];

			if (this.IsMatchedName(s, ref pos, ref n)) {
				c = s[pos++];

				if (c == '{') {
					var child = new Dead.Tree.Node.Child() {
						name     = n,
						value    = "",
						children = new List<Dead.Tree.Node.Child>(),
					};

					if (!this.ReadChildren(s, ref pos, ref child.children)) { return false; }

					children.Add(child);
				} else if (c == '=') {
#pragma warning disable CS8625 // null リテラルを null 非許容参照型に変換できません。
					var child = new Dead.Tree.Node.Child() {
						name     = n,
						value    = this.ReadValue(s, ref pos),
						children = null,
					};
#pragma warning restore CS8625 // null リテラルを null 非許容参照型に変換できません。

						children.Add(child);
				} else {
					pos--;
					return false;
				}
			} else if (c == ',') {	//あっても構わないが、ない方が良い
				pos++;
			} else if (c == '}') {	//子の終端を見つけたので親に戻す
				pos++;
				break;
			} else {	//<name>でも,でも}でもないので、エラーとして返す
				return false;
			}
		}

		return true;
	}
}

///////////////////////////////////////////////////////////////////////////////
}
