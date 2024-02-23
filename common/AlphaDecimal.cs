/*!
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。
 *         新しい文法に変更しないでください。
 */

using System;

namespace Dead {
///////////////////////////////////////////////////////////////////////////////

/*!
	@class AlphaDecimal
	@brief
	正の整数をExcelの列名に変換し、その逆変換も行う静的クラス。
	拡張メソッドで実装されているので、string, int, uint 型の変数や
	文字列リテラル、マジックナンバーに対して使うことができる。
	これらの設計も実装処理も、以下の記事を参考にして作った。
	http://zecl.hatenablog.com / entry / 20090206 / p1

	拡張メソッドを呼び出すには名前空間の参照が正しく行われている必要があるので、
	呼び出す前に using Dead が必要。
*/
public static class AlphaDecimal {

	public static string ToAlphabet(this int number) {
		if (number < 0) { return ""; }

		const int d = 26;
		int n = number % d;
		n = (n == 0) ? d : n;
		string s = ((char)(n + 64)).ToString();
		if(number == n) { return s; }

		return ((number - n) / d).ToAlphabet() + s;
	}

	public static string ToAlphabet(this uint number) {
		if (number < 0) { return ""; }

		return AlphaDecimal.ToAlphabet((int)number);
	}

	public static int ToInt(this string alphabet) {
		int result = 0;
		if (string.IsNullOrEmpty(alphabet)) { return result; }

		char[] chars = alphabet.ToCharArray();
		int len = alphabet.Length - 1;
		int asc;
		foreach(char c in chars) {
			asc = (int)c - 64;
			if (asc < 1 || asc > 26) { return 0; }

			result += asc * (int)System.Math.Pow((double)26, (double)len--);
		}

		return result;
	}

	public static uint ToUint(this string alphabet) {
		uint result = 0;
		if (string.IsNullOrEmpty(alphabet)) { return result; }

		char[] chars = alphabet.ToCharArray();
		int len = alphabet.Length - 1;
		uint asc;
		foreach(char c in chars) {
			asc = (uint)c - 64;
			if (asc < 1 || asc > 26) { return 0; }

			result += asc * (uint)System.Math.Pow((double)26, (double)len--);
		}

		return result;
	}
}

///////////////////////////////////////////////////////////////////////////////
}
