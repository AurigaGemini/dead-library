/*!
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。
 *         新しい文法に変更しないでください。
 */

using System.Text;
using System.Runtime.InteropServices;

namespace Dead {
///////////////////////////////////////////////////////////////////////////////

/*!
	iniファイルの読み書きを行うクラス。@n
	WIN32APIの関数を使うので、ウィンドウズ以外の環境で動作するかは不明。

	@note
	Read()メソッドについて。@n
	doubleとfloatの場合は変換できるかをチェックして、できた場合はその値を返し、できなかった場合はdefalut_valueが返る。@n
	文字列の場合はそのまま文字列に変換して返され、intの場合はWin32APIの戻り値をそのまま返す。@n
*/
public class Ini {
	[DllImport("KERNEL32.DLL")]
	static extern int GetPrivateProfileString(
		string lpAppName,
		string lpKeyName,
		string lpDefault,
		StringBuilder lpReturnedString,
		uint nSize,
		string lpFileName
		);

	[DllImport("KERNEL32.DLL")]
	static extern int GetPrivateProfileInt(
		string lpAppName,
		string lpKeyName,
		int nDefault,
		string lpFileName
		);

	[DllImport("KERNEL32.DLL")]
	static extern int WritePrivateProfileString(
		string lpAppName,
		string lpKeyName,
		string lpString,
		string lpFileName
		);

	/*!
		読み書きを行うiniファイルのパスを返す。
	*/
	public string Path { get; protected set; }

	/*!
		@params path ファイルが存在しない場合は書き込み時に自動的に生成される。
		        フルパスで指定しないと失敗する。
	*/
	public Ini(string path) { this.Path = path; }

	//	データ読み込み
	public string Read(string section, string key, string default_value, uint max_length) {
		var s = new StringBuilder((int)max_length);
		int r = Ini.GetPrivateProfileString(section, key, default_value, s, (uint)s.Capacity, this.Path);
		return s.ToString();
	}

	public int Read(string section, string key, int default_value) {
		return Ini.GetPrivateProfileInt(section, key, default_value, this.Path);
	}

	public float Read(string section, string key, float default_value) {
		string s = this.Read(section, key, "", 256);
		float f;
		if (float.TryParse(s, out f)) { return f; }

		return default_value;
	}

	public double Read(string section, string key, double default_value) {
		string s = this.Read(section, key, "", 256);
		double d;
		if (double.TryParse(s, out d)) { return d; }

		return default_value;
	}

	/*!
		値が true, yes, on, 1, + の場合は true を返す。@n
		false, no, off, 0, - の場合は false を返す。@n
		いずれにも当てはまらない場合は default_value を返す。
	*/
	public bool Read(string section, string key, bool default_value) {
		string s = this.Read(section, key, "", 8);
		s = s.ToLower();
		if (s == "true"  || s == "yes" || s == "on"  || s == "1" || s == "+") { return true;  }

		if (s == "false" || s == "no"  || s == "off" || s == "0" || s == "-") { return false; }

		return default_value;
	}

	//	データ書き込み
	public void Write(string section, string key, string write_value) {
		int r = Ini.WritePrivateProfileString(section, key, write_value, this.Path);
	}

	public void Write(string section, string key, int write_value) {
		int r = Ini.WritePrivateProfileString(section, key, write_value.ToString(), this.Path);
	}

	public void Write(string section, string key, double write_value) {
		int r = Ini.WritePrivateProfileString(section, key, write_value.ToString(), this.Path);
	}

	public void Write(string section, string key, float write_value) {
		int r = Ini.WritePrivateProfileString(section, key, write_value.ToString(), this.Path);
	}
}

///////////////////////////////////////////////////////////////////////////////
}
