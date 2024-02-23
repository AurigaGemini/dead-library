/*!
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。
 *         新しい文法に変更しないでください。
 */

using System;
using System.Reflection;

namespace Dead {
///////////////////////////////////////////////////////////////////////////////

/*!
	DLLをロードし、DLLに登録されているクラスのメソッドを呼び出す。

	手順
	1.コンストラクタに読み込むDLLのパスを指定。
	2.LoadClass()でDLL内のクラスを読み込む。
	3.CallMethod()で読み込んだクラスが持っているメソッドを呼び出す。
	   ※staticメソッドの場合も手順は同じ。
*/
public class DLL {

	readonly Assembly assembly;

	public object Instance { get; protected set; }

	public DLL(string dll_path) {
		this.assembly = Assembly.LoadFrom(dll_path);
	}

	/*!
		@params args 読み込むクラスのコンストラクタに指定する引数。
	 */
	public void LoadClass(string class_name, params object[] args) {
		Type t = this.assembly.GetType(class_name);
		this.Instance = Activator.CreateInstance(t, args);
	}
	public void LoadClass(string class_name) {
		Type t = this.assembly.GetType(class_name);
		this.Instance = Activator.CreateInstance(t);
	}

	/*!
		@params args 呼び出すメソッドに指定する引数。
	 */
	public object CallMethod(string method_name, params object[] args) {
		MethodInfo m = this.Instance.GetType().GetMethod(method_name);
		return m.Invoke(this.Instance, args);
	}
	public object CallMethod(string method_name) {
		MethodInfo m = this.Instance.GetType().GetMethod(method_name);
		return m.Invoke(this.Instance, new object[0]);
	}
}

///////////////////////////////////////////////////////////////////////////////
}
