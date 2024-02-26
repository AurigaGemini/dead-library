/*!
 * dead-library Dead.File.Collector
 * 
 * @version 1.0.0
 * 
 * @brief 指定したディレクトリ以下にある全てのファイルのパスを収集する機能を提供する。
 * 
 *   LISENCE
 *   https://github.com/AurigaGemini/dead-library/blob/main/LICENSE
 *   
 * @require
 *   NuGet package => System.Collections.Immutable (version >= 8.0.0)
 *   https://www.nuget.org/packages/System.Collections.Immutable/
 *   
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * 
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。@n
 *         新しい文法に変更しないでください。
 */

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace Dead.File {
///////////////////////////////////////////////////////////////////////////////////////////////////

sealed public class Collector {
	//! 収集が終わったときに呼び出すデリゲート
	public delegate void OnFinished(Collector caller);

	public Collector() {
		this.Initialize();
	}

	//! 収集が終わったら true を返す。エラーが起きた場合と Cancel した場合は true にならない。
	public bool IsCompleted { get; private set; }

	//! 収集時に何らかのエラーが起きた場合は true を返す。Cancel はエラーではない。
	public bool HasError { get; private set; }

	//! Cancel メソッドで中断した場合に true を返す。
	public bool IsCanceled { get; private set; }

	//! IsCompleted / HasError / IsCanceled のいずれかが true なら true を返す。
	public bool IsStopped => this.IsCompleted || this.HasError || this.IsCanceled;

	/*!
	 * 収集したファイル数を返す。
	 * @note 収集が終わる前に、その時点で収集したファイルの数を取得したい状況で使うことを想定したもの。
	 */
	public int Count => this.Files.Count;

	/*!
	 * 一番最後に収集した情報を返す。
	 * @note 収集が終わる前に、その時点で一番最後に収集したファイルの情報を取得したい状況で使うことを想定したもの。
	 */
	public string Last => this.Files.Last();

	/*!
	 * 収集した全てのファイルを ImmutableList で返す。
	 * @note 内部のキャッシュが保持している全要素を ImmutableList にコピーして返すため O(n) のコストがかかる。@n
	 *       計算量が膨大になることが想定される場合、このプロパティー呼び出しをタスクに登録し、非同期化することを推奨する。
	 * @note ディレクトリの走査に比べてキャッシュのコピーはコストが低いので、非同期コピーの機能を実装するかは未定。
	 */
	public ImmutableList<string> Result {
		get {
			ImmutableList<string> result = ImmutableList<string>.Empty;

			foreach (string file in this.Files) {
				//System.Console.WriteLine(file);
				result = result.Add(file);
			}

			//System.Console.WriteLine("Count={0}", result.Count);
			return result;
		}
	}

	/*!
	 * @brief ファイルの収集を開始する。@n
	 *        ファイルの収集は非同期で行われるので、呼び出し元の処理はブロックされず、
	 *        すぐに次の処理へ移行する。@n
	 *        従って、収集が終わる前に以下の状況が発生した場合、強制的に収集が中断される。@n
	 *        1. インスタンスが削除される。@n
	 *        2. プログラムまたはインスタンスを生成したスレッドが終了する。
	 * 
	 * @brief 収集が終わったかどうかは、以下のいずれかの方法で検出できる。@n
	 *        1. OnFinished デリゲートが Invoke される。@n
	 *        2. IsCompleted が true を返す。@n
	 *        3. HasError が true を返す。@n
	 *        4. IsCompletedOrHasError が true を返す。
	 *
	 * @param search_paths   = 収集を行うディレクトリ(フォルダ)のリスト。@n
	 *                         ディレクトリを指定した場合、直下にあるディレクトリを再帰的に調べる。@n
	 *                         再帰処理を行わないようにすることはできない。
	 * @param file_extention = 指定した拡張子のファイルだけを収集する。@n
	 *                         拡張子の前のピリオドはあってもなくてもどちらでも良い。@n
	 *                         全てのファイルを対象にする場合は空文字を指定する。
	 * @param on_finished    = 収集が終わったときに呼び出すメソッド。@n
	 *                         成功しても失敗しても呼び出される。@n
	 *                         成功時は IsCompleted が true になり、失敗時は HasError が true になる。
	 */
	public Task Start(List<string> search_paths, string file_extention = "", OnFinished on_finished = null) {
		if (this.canceler != null) {
			throw new System.InvalidOperationException("FileCollector already running.");
		}
		else {
			this.Initialize();

			if (search_paths == null || search_paths?.Count <= 0) {
				throw new System.ArgumentNullException(nameof(search_paths));
			}

			this.Paths         = search_paths;
			this.FileExtention = this.GetCorrectedExtention(file_extention);
			this.FinishedEvent = on_finished;
			this.canceler      = new CancellationTokenSource();

			return Task.Run(this.CollectFiles, this.canceler.Token);
		}
	}

	/*!
	 * 収集を中断する。
	 * @note 中断してもキャッシュは削除されないので Result で中断時の情報を取得できる。@n
	 *       中断した場合も OnFinished は Invoke され、IsCanceled が true になる。@n
	 *       収集中でない場合は何もしない。
	 */
	public void Cancel() {
		if (this.IsStopped) { return; }

		if (this.canceler == null) { return; }

		this.canceler.Cancel();

		this.IsCanceled = true;
		this.canceler   = null;
		this.FinishedEvent.Invoke(this);
	}

	///////////////////////////////////////////////////////////////////////////////////////////////
	//	以下、プライベートメンバー

	string        FileExtention { get; set; }
	Queue<string> Files         { get; set; }
	List<string>  Folders       { get; set; }
	List<string>  Paths         { get; set; }
	OnFinished    FinishedEvent { get; set; }

	CancellationTokenSource canceler;

	///////////////////////////////////////////////////////////////////////////////////////////////

	void Initialize() {
		this.FileExtention = string.Empty;
		this.Files         = new Queue<string>();
		this.Folders       = new List<string>();
		this.Paths         = new List<string>();
		this.FinishedEvent = null;
		this.canceler      = null;
		this.IsCompleted   = false;
		this.IsCanceled    = false;
		this.HasError      = false;
	}

	string GetCorrectedExtention(string extention_string) {
		string result = extention_string.Trim();
		if (string.IsNullOrEmpty(result)) { return string.Empty; }

		if (result[0] != '.') { return "." + result; }

		return result;
	}

	void CollectFiles() {
		this.Files.Clear();
		try {
			this.SearchPaths(this.Paths);

			while (this.Folders.Count > 0) {
				var folders = new List<string>();
				foreach (string folder in this.Folders) {
					folders.Add(folder);
				}

				this.Folders.Clear();
				this.SearchPaths(folders);
			}

			this.IsCompleted = true;
		}
		catch(System.Exception e) {
			System.Console.WriteLine("Error: {0}", e.ToString());
			this.HasError = true;
		}
		finally {
			this.canceler = null;
			this.FinishedEvent?.Invoke(this);
		}
	}

	void SearchPaths(List<string> paths) {
		foreach (string path in paths) {
			FileAttributes attribute = System.IO.File.GetAttributes(path);
			if ((attribute & FileAttributes.Directory) > 0) {
				//  ディレクトリ内のファイルを全て抜き出す
				this.GetAllFilesInDirectory(path);

				//  ディレクトリ内のサブディレクトリを全て抜き出す
				this.GetAllSubDirectories(path);
			}
			else {
				if (this.MustSkipFile(path)) {
					//	除外するファイルだった
					continue;
				}

				//  ファイルとして読み込む
				this.Files.Enqueue(path);
			}
		}
	}

	void GetAllFilesInDirectory(string path) {
		string[] files = Directory.GetFiles(path);
		foreach (string file in files) {
			if (this.Files.Contains(file)) { continue; }

			if (this.MustSkipFile(file)) { continue; }

			this.Files.Enqueue(file);
		}
	}

	void GetAllSubDirectories(string path) {
		string[] directories = Directory.GetDirectories(path);
		foreach (string directory in directories) {
			if (this.Folders.Contains(directory)) { continue; }

			this.Folders.Add(directory);
		}
	}

	bool MustSkipFile(string path) {
		string file_extention = Path.GetExtension(path);
		return !string.IsNullOrEmpty(this.FileExtention) && file_extention != this.FileExtention;
	}
}

///////////////////////////////////////////////////////////////////////////////////////////////////
}
