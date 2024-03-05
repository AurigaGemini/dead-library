/*!
 * dead-library Dead.Task.Rack
 * 
 * 架空の回路基板(ボードと呼称)と、それを収納できる架空のコンピューターラック(棚)を提供する。
 * 
 * @feature
 * ・１つの Task を架空のボード(回路基板)１枚として扱う。@n
 * ・棚に収納できるボードは枚数に制限がある。@n
 * ・１つの棚には同じ型 T のボードだけ収納できる。@n
 * ・ボードを棚に収納して電源を入れることでボードの処理が行われるようになる。@n
 * ・棚に収納したボードは取り外すことができる。@n
 * ・ボードの処理が終了しても棚から取り外すまで棚に残る。
 *
 * @usage
 * ・エクセルファイルを同時に 5 個まで並列で読み込みたい。@n
 * ・csv ファイルを 20 個まで同時に書き込みたい。@n
 * ・同時に行える HTTP リクエストを 10 個までに制限したい。@n
 * など…。
 * 
 * @require dead-library/ID.cs
 * @version 1.0.0
 * @lisence https://github.com/AurigaGemini/dead-library/blob/main/LICENSE
 * @note    .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark  DLL化して Unity などに組み込むため、あえて古い書き方をしています。@n
 *          新しい文法に変更しないでください。
 */

// 参照 DLL を減らしたいので Linq は使っていない
using System.Collections.Generic;
using System.Threading.Tasks;

using Canceler = System.Threading.CancellationTokenSource;

namespace Dead.Task {
///////////////////////////////////////////////////////////////////////////////////////////////////

/*!
 * @class 架空の回路基盤に取り付ける架空のチップとして機能するインターフェース
 * @brief チップは Task の処理を行う。@n
 *        継承クラスで Run メソッド実装し、Board クラスに取り付ける(コンストラクタの引数として渡す)ことで、
 *        Task を処理できるようになる。チップの処理を開始するには、ボードに電源を入れる必要があるが、
 *        電源を入れるには棚に収納する必要がある。@n
 *        ※棚に入れることで通電し、電源が入るようになる。@n
 *        ※電源が入ることで、ボードに取り付けたチップに記録されているバイナリコードが処理されるイメージ。
 */
public interface IChip<T> {
	/*!
	 * 継承先で実装し、ボードに組み込む処理
	 * @param  args チップに渡す値
	 * @remark args は T でなければならない。
	 */
	int Run(object args);

	//! Run メソッドの引数のキャッシュ
	T Data { get; set; }
}

/*!
 * @class Task を回路基板に見立てたクラス
 * @brief 回路基板にチップを取り付けることで処理できるようになる。@n
 *        回路基板の電源を入れることで取り付けたチップの処理が行われる。
 */
public class Board<T> {
	/*!
	 * @param chip Task を処理するインターフェースの継承クラス。@n
	 *             IChip<T> を継承し、Run メソッドを実装している必要がある。@n
	 *             棚には同じ型(T)のボードだけ収納することができる。
	 */
	public Board(IChip<T> ichip) {
		this.UniqueID = Dead.IncrementalIDGenerator.Invalid_ID;
		this.chip     = ichip;
		this.canceler = null;
		this.task     = null;
	}

	/*!
	 * ボードに電源を入れる（チップの処理を開始する）。
	 * @note  棚に収納していない場合は失敗する。@n
	 *        チップの処理が完了しても、以下のメソッドを呼ばなければ電源が入りっぱなしになる。
	 *        1. PowerOff メソッドを呼ぶ。
	 *        2. Eject メソッドを呼ぶ。
	 *        3. Destroy メソッドを呼ぶ。
	 * @param args チップに渡す値
	 */
	public virtual bool PowerOn(T args) {
		if (this.State == Status.PowerOn) { return false; } // 既に電源が入っている

		// 棚に収納することで UniqueID が付与されるので、初期値だった場合はまだ収納していない。
		if (this.UniqueID == Dead.IncrementalIDGenerator.Invalid_ID) { return false; }

		this.canceler  = new Canceler();
		this.task      = Task<int>.Factory.StartNew(this.chip.Run, args, this.canceler.Token);
		this.chip.Data = args;

		return true;
	}

	/*!
	 * ボードの電源を切る（チップの処理をキャンセルする）。
	 */
	public virtual bool PowerOff() {
		if (this.State != Status.PowerOn || this.canceler == null) {
			//System.Console.WriteLine("PowerOff : false / State={0} canceler={1}", this.State, this.canceler);
			return false;
		}

		this.canceler.Cancel();

		this.canceler  = null;
		this.chip.Data = default;

		//System.Console.WriteLine("PowerOff : true");
		return true;
	}

	//! ボードの状態を示す値
	public enum Status {
		None,
		PowerOn,
		Completed,
		Canceled,
		Error,
	}

	//! ボードの状態を返す。
	public virtual Status State {
		get {
			if (this.debugStatus != Status.None) {
				System.Console.WriteLine("debugStatus={0}", this.debugStatus);
				return this.debugStatus;
			}

			if (this.task == null) { return Status.None; }

			if (this.task.IsCompleted) { return Status.Completed; }

			if (this.task.IsCanceled ) { return Status.Canceled; }

			if (this.task.IsFaulted  ) { return Status.Error; }

			return Status.PowerOn;
		}
	}

	//! チップの終了コードを返す。電源を入れていない場合は int.MaxValue を返す。
	public virtual int ExitCode {
		get {
			if (this.task != null) { return this.task.Result; }

			return int.MaxValue;
		}
	}

	/*!
	 * ボードに割り振られる一意な ID
	 * @remark ID は棚が割り振るので、このクラスや棚以外のクラスで割り振らないでください。
	 */
	public virtual ulong UniqueID { get; set; }
	
	//! デバッグ用：None 以外を指定すると State プロパティーがその値を返すようになる。
	public virtual void DebugForceState(Status status) {
		this.debugStatus = status;
	}

	//! Task で処理するメソッドと Task の起動引数を保持
	protected IChip<T> chip;
	
	//! Task の処理を強制終了するためのオブジェクト
	protected Canceler canceler;

	//! Task の実体
	protected Task<int> task;

	//! DebugForceState メソッドのコメント参照
	protected Status debugStatus = Status.None;
}

/*!
 * @class ボードを収納する棚として機能するクラス
 * @brief ボードは棚に収納し、電源を入れることで処理が行われる。
 */
public class Rack<T> {
	//! board_id が無効値(エラーとして扱う値)なら true を返す。
	public static bool IsErrorID(ulong board_id) {
		return board_id == Dead.IncrementalIDGenerator.Invalid_ID;
	}

	/*!
	 * @param max_number 棚に収納できるボードの枚数。@n
	 *                   この数は変更できない。
	 */
	public Rack(byte max_number) {
		this.Initialize();
		this.MaxNumber = max_number;
	}

	//! 収納できるボードの最大数
	public virtual byte MaxNumber { get; protected set; }

	//! 棚にボードを収納する空きがあれば true を返す。
	public virtual bool HasSpace => this.boards.Count < this.MaxNumber;

	//! 棚に収納しているボードの数を返す。
	public virtual int Count => this.boards.Count;

	//! 棚に収納している全てのボードの処理が完了していれば true を返す。
	public virtual bool IsAllCompleted {
		get {
			foreach (Board<T> board in this.boards) {
				if (board.State != Board<T>.Status.Completed) { return false; }
			}

			return true;
		}
	}

	//! board_id と一致するボードが棚に収納されているなら true を返す。
	public virtual bool HasBoard(ulong board_id) {
		foreach (Board<T> board in this.boards) {
			if (board.UniqueID == board_id) {
				return true;
			}
		}

		return false;
	}

	/*!
	 * ボードにユニークIDを付与し、棚に収納する。
	 * @return 収納できた場合は有効なユニークIDを返す。@n
	 *         収納できなかった場合は Dead.IncrementalIDGenerator.InvalidID を返す。
	 * @note   戻り値のユニークIDは、棚に収納しているボードにアクセスする際に必要になる。
	 */
	public virtual ulong Insert(Board<T> board) {
		bool is_failed_insert = !this.InsertInternal(board);
		if (is_failed_insert) { return Dead.IncrementalIDGenerator.Invalid_ID; }

		board.UniqueID = Rack<T>.idGenerator.Generate();
		this.boards.Add(board);
		return board.UniqueID;
	}

	/*!
	 * 棚に収納したボードの電源を入れる。
	 * @note 電源を入れることでチップ(Task)の処理が行われるようになる。
	 */
	public virtual bool PowerOn(ulong board_id, T args) {
		foreach (Board<T> board in this.boards) {
			if (board.UniqueID != board_id) { continue; }

			if (board.State == Board<T>.Status.PowerOn) { return false; }

			bool is_succeeded = board.PowerOn(args);
			return is_succeeded;
		}

		return false;
	}

	/*!
	 * 棚に収納したボードの電源が入っていれば、電源を切る。
	 * @return board_id と一致するボードがない、または、チップが処理中でない場合、false を返す。
	 */
	public virtual bool PowerOff(ulong board_id) {
		foreach (Board<T> board in this.boards) {
			if (board.UniqueID != board_id) { continue; }

			if (board.State == Board<T>.Status.PowerOn) {
				return board.PowerOff();
			}

			break;
		}

		return false;
	}

	/*!
	 * 全てのボードの電源を切る。
	 * @param  target_status None 以外を指定した場合、指定したステータスと一致するボードのみ電源を切る。
	 *                       None を指定するか省略した場合、ボードのステータスに関わらず全てのボードの電源を切る。
	 * @return PowerOff に成功したボードの数を返す。
	 */
	public virtual int PowerOffAll() {
		int result = 0;

		foreach (Board<T> board in this.boards) {
			result += board.PowerOff() ? 1 : 0;
		}

		//System.Console.WriteLine("PowerOffAll() succeeded board number = {0}", result);
		return result;
	}

	/*!
	 * 棚からボードを取り出し、ボード一枚分の空きを作る。
	 * @note   棚から取り出したボードは電源が切られることで処理が中断され、保持しているIDが無効値になる。
	 * @return board_id と一致するボードを返す。@n
	 *         board_id と一致するボードが棚になかった、あるいは、棚にあるボードを破棄できなかった場合 null を返す。
	 */
	public virtual Board<T> Eject(ulong  board_id) {
		foreach (Board<T> board in this.boards) {
			if (board.UniqueID ==  board_id) {
				bool is_failed_to_remove = !this.boards.Remove(board);
				if (is_failed_to_remove) { break; }

#pragma warning disable	IDE0058	//式の値が未使用
				board.PowerOff();
#pragma warning restore IDE0058	//式の値が未使用

				board.UniqueID = Dead.IncrementalIDGenerator.Invalid_ID;

				return board;
			}
		}

		return null;
	}

	/*!
	 * 棚にある全てのボードの電源を切ってから破棄する。棚が空になる。
	 */
	public virtual void Destroy() {
		foreach (Board<T> board in this.boards) {

#pragma warning disable	IDE0058	//式の値が未使用
			board.PowerOff();
#pragma warning restore IDE0058	//式の値が未使用

		}

		this.boards.Clear();
	}

	/*!
	 * ボードに取り付けているチップの終了コードを返す。
	 * @return チップが終了コードを返していない場合、int.MinValue を返す。
	 */
	public virtual int GetExitCode(ulong board_id) {
		foreach (Board<T> board in this.boards) {
			if (board.UniqueID != board_id) { continue; }

			if (board.State == Board<T>.Status.Completed) { return board.ExitCode; }

			break;
		}

		return int.MinValue;
	}

	/*!
	 * 棚に収納しているボードを返す。
	 * @return board_id で指定したボードが棚に収納されていない場合は null を返す。
	 */
	public virtual Board<T> GetBoard(ulong board_id) {
		foreach (Board<T> board in this.boards) {
			if (board.UniqueID != board_id) { continue; }

			return board;
		}

		return null;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////
	//	以下、protected メンバー

	protected virtual bool InsertInternal(Board<T> board) {
		if (board == null) { return false; }

		bool can_not_insert = !this.HasSpace;
		if (can_not_insert) { return false; }	

		foreach (Board<T> board_in_rack in this.boards) {
			if (board == board_in_rack) {
				return false;
			}
		}

		return true;
	}

	protected List<Board<T>> boards;

#pragma warning disable S2743 // A static field in a generic type is not shared among instances of different close constructed types.
	//! ボードに割り当てる一意な ID を生成するジェネレーター。T が異なれば ID が重複しても構わない。
	protected static readonly Dead.IncrementalIDGenerator idGenerator = new Dead.IncrementalIDGenerator();
#pragma warning restore S2743 // A static field in a generic type is not shared among instances of different close constructed types.

	protected void Initialize() {
		this.MaxNumber = 0;
		this.boards    = new List<Board<T>>();
	}
}

///////////////////////////////////////////////////////////////////////////////////////////////////
}