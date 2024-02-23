/*!
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。
 *         新しい文法に変更しないでください。
 */

using System;
using System.Runtime.Serialization;

namespace Dead {
///////////////////////////////////////////////////////////////////////////////

/*!
	任意のタイミングで値の変更を制限するクラス。@n
	定数の代わりに使う。@n
	一度ロックしたら解除することはできない。@n
	ロック後に変更しようとすると例外が投げられる。@n
	オブジェクト型を登録した場合、ロックしてもフィールドやメソッドへの
	アクセスに制限はかからない。@n
	このクラスのValueフィールドにsetしようとした場合のみ制限される。
*/
sealed public class Constraint<T> {

	[Serializable]
	sealed public class LockedValueException : System.Exception {
		public LockedValueException() : base() { }
		LockedValueException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	public bool IsLocked { get; private set; }

	T managedValue;

	public T Value {
		get => this.managedValue;
		set {
			if (this.IsLocked) { throw new LockedValueException(); }

			this.managedValue = value;
		}
	}

	public System.Type ValueType => typeof(T);

	public Constraint(T initial_value = default) {
		this.managedValue = initial_value;
	}

	public void Lock() {
		this.IsLocked = true;
	}

	public static implicit operator T(Constraint<T> c) => c.Value;

	public override string ToString() { return this.Value.ToString(); }
}

///////////////////////////////////////////////////////////////////////////////
}
