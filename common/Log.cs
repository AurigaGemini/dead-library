/*!
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。
 *         新しい文法に変更しないでください。
 */

namespace Dead {
///////////////////////////////////////////////////////////////////////////////

/*!
	@class Log
	指定したテキストファイルにログを出力する。@n
	ログには、呼び出し元のファイル名、名前空間、クラス名、メソッド名、行数、
	列数が付加される。@n
	ただし、ファイル名、行数、列数は /debug:+ オプションを付けて
	コンパイルしなければ取得できない。@n
	Add() は System.Console.WriteLine() と同じようにフォーマット形式の
	文字列とパラメータの組み合わせを扱うことができる。@n
	このクラスのインスタンスが存在している間、テキストファイルは
	読み取り可能、書き込み不可のロック状態になる。

	@remarks /define:DEBUG_LOG を指定すると、Add()したログが標準出力にも表示される。

*/
public class Log {
	private System.IO.StreamWriter _f;

	public bool Append { get; set; }

	public string Path { get; protected set; }

	public Log(string file_path) {
		this.Path = file_path;
		this.Open();
	}

	public virtual void Add(string log, params object[] args) {
		if (log.Length < 1) { return; }

		if (this._f == null) { this.Open(); }

		var st = new System.Diagnostics.StackTrace(1, true);
		System.Diagnostics.StackFrame sf = st.GetFrame(0);
		
		int col = sf.GetFileColumnNumber();
		int row = sf.GetFileLineNumber();

		string s = "[" + sf.GetFileName() + "] " + sf.GetMethod().DeclaringType + "." + sf.GetMethod().Name + "(" + row + "," + col + ") : " + string.Format(log, args) + "\r\n";
	#if DEBUG_LOG
		Utility.Print(s);
	#endif
		this._f.Write(s);
	}

	public virtual void AddAndClose(string log, params object[] args) {
		this.Add(log, args);
		this.Close();
	}

	protected virtual void Open() {
		try {
			this._f = new System.IO.StreamWriter(this.Path, this.Append,  System.Text.Encoding.Unicode) {
				AutoFlush = true
			};
		} catch(System.Exception) {
			this.Close();
		}
	}

	protected virtual void Close() {
		if (this._f != null) {
			this._f.Dispose();
			this._f = null;
		}
	}
}

///////////////////////////////////////////////////////////////////////////////
}
