
///////////////////////////////////////////////////////////////////////////////
#pragma warning disable IDE0161 // 範囲指定されたファイルが設定された namespace に変換
namespace Dead {
#pragma warning restore IDE0161 // 範囲指定されたファイルが設定された namespace に変換
///////////////////////////////////////////////////////////////////////////////

public interface IService {
	bool IsProvided { get; set; }
}

/*!
 * @class  サービスロケータ
 * @brief  グローバルにアクセスできるサービスを提供するクラス。
 * @note   シングルトンと似ているが、インスタンスは複数ある可能性がある。@n
 *         その中のひとつをグローバルアクセスできるようにするための仕組み。
 * @remark Unity の DLL として使えるよう、.Net Standard 2.0 に合わせて記述している。
 * @see    書籍 Game Programming Patterns ソフトウェア開発の問題解決メニュー
 * @see    https://youtu.be/0LC5BgwPKOc
 */
#pragma warning disable CS8625 // null リテラルを null に変換できません。
public static class ServiceLocator<T> {
	static IService service = default;
	static bool isRegistered = false;

	/*!
	 * @brief サービスのインスタンスを返す。
	 * @note  サービスを提供していない場合は、T.IsProvided == false になる。
	 */
	public static T GetService() {
		ServiceLocator<T>.service.IsProvided = ServiceLocator<T>.isRegistered;
		return (T)service;
	}

	/*!
	 * @brief  ServiceLocator でサービスを提供できる状態にする。
	 * @note   IService を継承したクラスなら何でも良い。
	 * @remark このメソッドに登録することで IService.IsProvided == true になる。
	 */
	public static void Provide(IService service) {
		ServiceLocator<T>.service = service;
		ServiceLocator<T>.isRegistered = true;
	}

	/*!
	 * @brief ServiceLocator からのサービスの提供を差し控える。
	 * @remark このメソッドに登録することで IService.IsProvided == true になる。
	 */
	public static void Withhold() {
		ServiceLocator<T>.service = default;
		ServiceLocator<T>.isRegistered = false;
	}
}
#pragma warning restore CS8625 // null リテラルを null に変換できません。

///////////////////////////////////////////////////////////////////////////////
}
///////////////////////////////////////////////////////////////////////////////
