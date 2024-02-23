/*!
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。
 *         新しい文法に変更しないでください。
 */

namespace Dead {
///////////////////////////////////////////////////////////////////////////////

/*!
	64bitのインクリメンタルIDを生成して返す。@n
	使い終わったIDの再利用はしない。@n
	再利用する必要があるなら、別途作る。
	@note
	最大値は 18,446,744,073,709,551,615 なので、1ミリセカンドに1つのIDを
	発行した場合、IDを消費するのに 18,446,744,073,709,551 秒かかる。@n
	307,445,734,561,825分=5,124,095,576,030時間=213,503,982,334日=584,942,417年@n
	現実的に消費は不可能。仮にID/μ秒であっても584,942年必要。
*/
public class IncrementalIDGenerator {

	class ExhaustedIDException : System.Exception {}

	public const ulong InvalidID = 0;
	public const ulong InitialID = 1;
	ulong id;

	/*!
		@param start_value 途中まで発行したIDの続きから再開したい場合に指定する。@n
		                   省略時は INITIAL_ID から開始する。
	*/
	public IncrementalIDGenerator(ulong start_value = IncrementalIDGenerator.InitialID) {
		this.id = start_value;
	}

	public static bool IsInvalidID(ulong id_for_test) {
		return id_for_test == IncrementalIDGenerator.InvalidID ? true : false;
	}

	public virtual ulong Generate() {
		if (this.id < 0xFFffFFffFFffFFff) { return this.id++; }

		throw new ExhaustedIDException();	//まず起きない
	}

	/*!
		発行するIDの値を _INITIAL_ID にセットする。
		@warning IDの値はコンストラクタに指定する start_value になるわけではない。@n
		         start_value とは無関係なので注意。IDの値は必ず _INITIAL_ID になる。
	*/
	public void Reset() {
		this.id = IncrementalIDGenerator.InitialID;
	}
}

///////////////////////////////////////////////////////////////////////////////
}
