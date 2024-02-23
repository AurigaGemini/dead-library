/*!
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。
 *         新しい文法に変更しないでください。
 */

namespace Dead.String {
///////////////////////////////////////////////////////////////////////////////

public static class Utility
{
	public static bool IsAlphabet(char c) {
		return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' ? true : false;
	}
	public static bool IsNumber(char c) {
		return c >= '0' && c <= '9' ? true : false;
	}
	public static bool IsAlphaNum(char c) {
		return IsAlphabet(c) || IsNumber(c) ? true : false;
	}
	public static bool IsMark(char c) {	//制御文字を除いた記号
		return c >= 0x21 && c <= 0x2f || c >= 0x3a && c <= 0x40
			|| c >= 0x5b && c <= 0x60 || c >= 0x7b && c <= 0x7e
			? true : false;
	}
	public static bool IsSpace(char c) {
		return c == ' ' || c == '\t' || c == '\r' || c == '\n' ? true : false;
	}
	public static bool IsReturn(char c) {
		return c == '\r' || c == '\n' ? true : false;
	}
}

public class Analyzer
{
	/*!
		改行コード(CRLF、または、CRまたはLF)の次の位置まで read_position の
		位置を進める。
	*/
	public static void ReadLineEnd(string s, ref int read_position) {
		for (; read_position < s.Length; read_position++) {
			if (ReadReturnOnce(s, ref read_position)) { return; }
		}
	}

	/*!
		空白がなくなるまで read_position の位置を進める。
		read_position の位置は空白以外が検出された位置で返される。
	*/
	public static void ReadSpace(string s, ref int read_position) {
		for (; read_position < s.Length; read_position++) {
			if (ReadSpaceOnce(s, ref read_position)) { continue; }

			break;
		}
	}

	/*!
		s の read_position の位置が空白もしくは改行なら
		空白もしくは改行１つ分 read_position の位置を進めて true を返す。
		そうでないなら false を返す。
	*/
	public static bool ReadSpaceOnce(string s, ref int read_position) {
		if (ReadReturnOnce(s, ref read_position)) { return true; }

		if (Dead.String.Utility.IsSpace(s[read_position])) {
			read_position++;
			return true;
		}

		return false;
	}

	/*!
		s の read_position の位置が改行なら、改行１つ分 read_position の位置を
		進めて true を返す。
		そうでないなら false を返す。
		改行は CRLF、CR、LF のいずれか。
		即ち、CRLFが見つかった場合 read_position の位置は +2 される。
	*/
	public static bool ReadReturnOnce(string s, ref int read_position) {
		if (read_position < s.Length - 1 && s[read_position] == '\r' && s[read_position + 1] == '\n') {
			read_position += 2;
			return true;
		}

		if (Dead.String.Utility.IsReturn(s[read_position])) {
			read_position++;
			return true;
		}

		return false;
	}

	/*!
		read_position の位置まで改行がいくつあるか数えて返す。
		文字列がおかしい場合や何らかのエラーが起きた場合は負の値を返す。
	*/
	public static int GetReturnCount(string s, int read_position) {
		int count = 0;
		for (int i = 0; i <= read_position; i++ ) {
			if (ReadReturnOnce(s, ref i)) {
				count++;
			}
		}

		return count;
	}

	/*!
		read_position の位置が何行目の何桁目に当たるかを調べ
		col と row に格納して返す。
		col と row は自然数なので、0スタートにするなら -1 すること。
	*/
	public static void GetColAndRow(string s, int read_position, ref int col, ref int row) {
		col = 1;
		row = 1;
		int i = 0;
		while (i <= read_position && i < s.Length) {
			if (ReadReturnOnce(s, ref i)){
				row++;
				col = 1;
			} else {
				col++;
				i++;
			}
		}
	}
}

///////////////////////////////////////////////////////////////////////////////
}
