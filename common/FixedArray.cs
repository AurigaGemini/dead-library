/*!
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。
 *         新しい文法に変更しないでください。
 */

using System.Collections.Generic;

namespace Dead {
///////////////////////////////////////////////////////////////////////////////

/*!
	基本、固定サイズとして扱う配列。@n
	要素を削除しても、そのインデックスが空きリストに追加されるだけで、
	要素数は変わらない。@n
	固定サイズのデータかつ頻繁に要素を削除する用途に適している。@n
	拡張は可能。System.Array.Resize()を使って拡張される。@n
	パフォーマンスはSystem.Array.Resize()の能力次第だが、配列を拡張する場合@n
	要素の全コピーは避けられないので、多分、遅い。
	メリット
		削除が速い
		１次元配列なのでアクセスが速い（はず）
	デメリット
		全インデックスの取得が遅い
		全データの取得が遅い
		パフォーマンスが未検証
*/
public class FixedArray<T> : ArrayEnumeratorBase<T>
{
	public class InvalidElement : System.Exception {
		public InvalidElement() : base("要素が削除されて無効になっているか、まだ割り当てられていない状態です。\nこの要素へアクセスする前に Add(index) を使用して割り当てて下さい。") {
		}
	}

	readonly Queue<uint> blanks = new Queue<uint>();	//!	未使用のインデックスを管理するキュー
	
	public uint Count => (uint)(base.Length - this.blanks.Count);

	public uint Capacity => (uint)base.Length;

	/*!
		使用しているインデックスの配列を返す。
		@warning 遅い。
		@todo    高速化。
	*/
	public uint[] Indexes {
		get {
			if (this.Count < 1) { return null; }

			uint[] indexes = new uint[this.Count];
			uint write = 0;
			uint length = base.Length;
			for (uint i = 0; i < length; i++) {
				if (!this.blanks.Contains(i)) { indexes[write++] = i; }
			}

			return indexes;
		}
	}

	/*!
		使用している値の配列を返す。
		@warning 遅い。
		@todo    高速化。
	*/
	public T[] Values {
		get {
			if (this.Count < 1) { return null; }

			var values = new T[this.Count];
			uint write = 0;
			uint length = base.Length;
			for (uint i = 0; i < length; i++) {
				if (!this.blanks.Contains(i)) { values[write++] = base.Get(i); }
			}

			return values;
		}
	}

	public FixedArray(uint size) : base(new T[size]) {
		uint length = base.Length;
		for (uint i = 0; i < length; i++) { this.blanks.Enqueue(i); }
	}

	public void Clear() {
		this.blanks.Clear();
		uint length = base.Length;
		for (uint i = 0; i < length; i++) { this.blanks.Enqueue(i); }
	}

	/*!
		使用する要素は必ずこのメソッドを使って予約する。
		未使用の要素があればそのインデックス返し、
		ない場合は配列のサイズを拡張して追加した要素のインデックスを返す。
		使用済みのインデックスは Cancel(index) で返却することで、
		返却したインデックスを再利用できるようになる。
	 */
	public uint Reserve(T data) {
		if (this.blanks.Count > 0) {
			uint index = this.blanks.Dequeue();
			base.Set(index, data);
			return index;
		} else {
			uint new_index  = base.Length;
			uint new_length = new_index + 1;
			base.Resize(new_length);
			base.Set(new_index, data);
			return new_index;
		}
	}

	/*!
		指定したインデックスの要素を未使用状態に戻す。
		以降、未使用状態の要素を利用するには Reserve() を使用する必要がある。
		ただし、直前の Cancel() で返却した index を再度取得できるとは限らない。
		返却できた場合は true を返す。
	 */
	public bool Cancel(uint index) {
		if (index >= base.Length)  { return false; }	//indexが範囲外

		if (this.blanks.Contains(index)) { return false; }	//既に返却済み

		this.blanks.Enqueue(index);
		return true;
	}

	public T this[uint index] {
		get {
			if (index >= base.Length)  { throw new System.IndexOutOfRangeException(); }

			if (this.blanks.Contains(index)) { throw new InvalidElement(); }

			return base.Get(index);
		}
		set {
			if (index >= base.Length)  { throw new System.IndexOutOfRangeException(); }

			if (this.blanks.Contains(index)) { throw new InvalidElement(); }

			base.Set(index, value);
		}
	}
}

///////////////////////////////////////////////////////////////////////////////
}
