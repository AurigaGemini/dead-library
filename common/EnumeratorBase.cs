/*!
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。
 *         新しい文法に変更しないでください。
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Dead {
///////////////////////////////////////////////////////////////////////////////

/*!
	一次元配列用の Enumerator

 */
public class ArrayEnumeratorBase<T> : IEnumerable<T> {
	public class Enumerator<U> : IEnumerator<U>, IDisposable {
		//	この Enumerator が動作するのに必要なデータとコンストラクタ
		U[] array = null;
		int current = -1;
		int size;

		public Enumerator(U[] source) { this.array = source; this.size = this.array.Length; }

		//	この Enumerator が処理を行う際、外部から呼び出されるメソッドとプロパティ
		public bool MoveNext() { return ++this.current < this.size; }
		public void Reset() { this.current = -1; }
		public U Current => this.array[this.current];
		object IEnumerator.Current => this.Current;
		void IDisposable.Dispose() { this.array = null; this.current = -1; this.size = 0; }

		public U this[uint index] {
			get {
				if (index >= this.array.Length) { throw new System.IndexOutOfRangeException(); }

				return this.array[index];
			}
			set {
				if (index >= this.array.Length) { throw new System.IndexOutOfRangeException(); }

				this.array[index] = value;
			}
		}

		public uint Length => (uint)this.size;
		public void Resize(uint new_length) {
			System.Array.Resize(ref this.array, (int)new_length);
		}
	}

	// IEnumerable<T> のメソッド
	public IEnumerator<T> GetEnumerator() { return this.enumerator; }

	// コンストラクタ
	protected ArrayEnumeratorBase(T[] source) { this.enumerator = new Enumerator<T>(source); }

	// IEnumerable<T> が IEnumerable を継承しているため実装しなければコンパイルが通らないが
	// このメソッドが呼ばれることはないので、外部公開する必要はなく、有効な値を返す必要もない。
	IEnumerator IEnumerable.GetEnumerator() { return null; }

	// 配列のリサイズ時などに使用
	protected void Reset(T[] source) { this.enumerator = new Enumerator<T>(source); }

	//	配列を指定した要素数にリサイズする
	protected void Resize(uint new_length) { this.enumerator.Resize(new_length); }

	// 配列の要素数を返す
	protected uint Length => this.enumerator.Length;

	protected T Get(uint index) { return this.enumerator[index]; }
	protected void Set(uint index, T value) { this.enumerator[index] = value; }

	protected T Get(int index) {
		if (index < 0) { throw new System.IndexOutOfRangeException(); }

		return this.enumerator[(uint)index];
	}
	protected void Set(int index, T value) {
		if (index < 0) { throw new System.IndexOutOfRangeException(); }

		this.enumerator[(uint)index] = value;
	}

	Enumerator<T> enumerator;
}

///////////////////////////////////////////////////////////////////////////////
}
