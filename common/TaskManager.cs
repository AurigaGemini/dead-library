
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dead.Task;

/*!
	タスクを一丸管理するためのクラス

	ThreadPool では個別のタスクにアクセスできないため作成。
	.Net の Task を内部で使う形で実装したが、以下の仕様に合わないないため Action を持つ形に変更した。
	※Action と識別用の情報だけ持っていれば良く、Task を使う必要がない。

	・毎フレーム１回処理を実行するようなゲーム用途を想定。
	・毎フレーム都合の良いタイミングで Manager.Update を呼び出すことでタスクを実行。
	・タスクを実行してもタスクは破棄されない（明示的に破棄するまで Update でタスクが実行される）。
*/
/*!




	@warning テストしてません！！！




*/
static public class Manager {
	public class TaskActionUsedTooLongTimeException : System.Exception {}

	/*!
		category と name の両方が一致する場合に、同じタスクと判別されます。
	*/
	public struct Property {
		public uint   category;	/// タスクの処理内容を表す数値 0-メインループ 1-キャラ 2-UI など…
		public string name;     /// タスクを識別するための名前
		public Property(uint category, string name) {
			this.category = category;
			this.name     = name;
		}
	}

	public enum Status {
		Created,	/// タスクを生成しているがまだ実行していない
		Started,	/// 少なくとも１回は実行した
		Stopped,	/// 実行を停止中
		Removed,	/// 削除マークを設定したがまだ削除はされていない
	}

	/*!
		@brief 登録されているタスクの数を返す。
	*/
	static public int Count => Manager.datas.Count;

	/*!
		@brief 登録されているタスクを実行する。
		@note  ステータスが Created か Started の場合のみ実行します。
		       Removed の場合はタスクを実行せず、他の全てのタスクを実行した後で削除します。
	*/
	static public void Update(float timeout_second = 0.01f, bool throw_exception_if_timeout = true, bool is_debug = false) {
		//	datas をイテレート中に削除して例外が出るなどすると後始末が面倒なので
		//	削除マークがついたタスクをリストに登録しておき、最後にまとめて削除する
		var remove_list = new List<Manager.Data>();

		//	DateTime を var にすると IDE0008 が出る…。
		DateTime update_start_time = DateTime.Now;
		foreach (Manager.Data data in Manager.datas) {
			if (data.status == Manager.Status.Created || data.status == Manager.Status.Started) {
				if (is_debug) {
					//	デバッグ指定時は個別のタスク処理にかかった時間を計測してログを取る
					//	標準出力に出力しているが、Logger と絡めた方が良さそう
					DateTime task_start_time = DateTime.Now;
					data.task();
					DateTime task_end_time = DateTime.Now;
					TimeSpan task_elapsed_time = task_end_time - task_start_time;
					Console.WriteLine("task:\n\tcategory=" + data.property.category + "\n\tname=" + data.property.name + "\n\telapsed_time=" + task_elapsed_time.Milliseconds);
				}
				else {
					data.task();
				}

				TimeSpan elapsed_time = DateTime.Now - update_start_time;
				if (elapsed_time.Milliseconds > timeout_second * 1000) {
					if (throw_exception_if_timeout) { throw new TaskActionUsedTooLongTimeException(); }
					else { return; }
				}

				data.status = Manager.Status.Started;
			}
			else if (data.status == Manager.Status.Removed) {
				remove_list.Add(data);
			}
		}

		foreach (Manager.Data data in remove_list) {
			//	戻り値を _ に代入しているのは IDE0058 対策
			_ = Manager.datas.Remove(data);
		}
	}

	static public void Clear() {
		Manager.datas.Clear();
	}

	/*!
		@brief  タスクを生成して返す。
		@note   生成したタスクは Start() を呼ばない限り実行されない。
		@return 生成に失敗した場合は null を返す。
	*/
	static public bool Add(Manager.Property p, Action a) {
		if (Manager.HasSameTask(p)) { return false; }

		var task = new Manager.Data(p, a);
		Manager.datas.Add(task);
		return true;
	}

	/*!
		@brief 指定したプロパティーを持つタスクを削除する。
		@note  ここでは削除マークをつけるだけで、実際に削除するのは Update で行います。
	*/
	static public bool Remove(Manager.Property _) {
		Manager.Data? data = GetFromProperty(_);
		if (data == null) { return false; }

		data.status = Manager.Status.Removed;
		return true;
	}

	/*!
		@brief 指定した名前を持つタスクを削除する。
		@note  ここでは削除マークをつけるだけで、実際に削除するのは Update で行います。
	*/
	static public bool Remove(string task_name) {
		Manager.Data? data = GetFromName(task_name);
		if (data == null) { return false; }

		data.status = Manager.Status.Removed;
		return true;
	}

	static public bool Start(Manager.Property _) {
		Manager.Data? data = Manager.GetFromProperty(_);
		if (data == null) { return false; }

		data.status = Manager.Status.Created;
		return true;
	}

	static public bool Start(string task_name) {
		Manager.Data? data = Manager.GetFromName(task_name);
		if (data == null) { return false; }

		data.status = Manager.Status.Created;
		return true;
	}

	static public bool Stop(Manager.Property _) {
		Manager.Data? data = Manager.GetFromProperty(_);
		if (data == null) { return false; }

		data.status = Manager.Status.Stopped;
		return true;
	}

	static public bool Stop(string task_name) {
		Manager.Data? data = Manager.GetFromName(task_name);
		if (data == null) { return false; }

		data.status = Manager.Status.Stopped;
		return true;
	}

	static public bool HasSameTask(Manager.Property _) {
		foreach (Manager.Data data in Manager.datas) {
			if (Manager.IsSameTask(data.property, _)) { return true; }
		}

		return false;
	}

	static public bool IsSameTask(Manager.Property p1, Manager.Property p2) {
		return p1.category == p2.category && p1.name == p2.name ? true : false;
	}

	//////////////////////////////////////

	static Manager.Data? GetFromName(string task_name) {
		foreach (Manager.Data data in Manager.datas) {
			if (data.property.name == task_name) { return data; }
		}

		return null;
	}

	static Manager.Data? GetFromProperty(Manager.Property _) {
		foreach (Manager.Data data in Manager.datas) {
			if (Manager.IsSameTask(data.property, _)) { return data; }
		}

		return null;
	}

	class Data {
		public Manager.Property property;

		public Manager.Status   status;

		public Action task;

		public Data(Manager.Property p, Action t) {
			this.property = p;
			this.task     = t;
			this.status   = Manager.Status.Created;
		}
	}

	static List<Manager.Data> datas = new ();
}
