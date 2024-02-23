/*!
 * @note   .Net Standard 2.0(C# 7) に合わせて記述しているため、文法が古いです。
 * @remark DLL化して Unity などに組み込むため、あえて古い書き方をしています。
 *         新しい文法に変更しないでください。
 */

namespace Dead {
///////////////////////////////////////////////////////////////////////////////

public static class WindowsMediaPlayer {
	static dynamic wmp = null;

	public static void Create() {
		if (WindowsMediaPlayer.wmp == null) {
			WindowsMediaPlayer.wmp = System.Activator.CreateInstance(System.Type.GetTypeFromProgID("WMPlayer.OCX.7"));
		}
	}

	public static bool IsPlaying { get; set; } = false;

	public static bool IsNotPlaying => !WindowsMediaPlayer.IsPlaying;

	public static void Play(string path) {
		if (WindowsMediaPlayer.wmp == null) { return; }

		if (WindowsMediaPlayer.IsPlaying) { return; }

		if (WindowsMediaPlayer.IsExisted(path)) {
			WindowsMediaPlayer.wmp.URL = path;
			WindowsMediaPlayer.wmp.controls.Play();
		}
	}

	public static void Stop() {
		if (WindowsMediaPlayer.wmp == null) { return; }

		if (WindowsMediaPlayer.IsNotPlaying) { return; }

		WindowsMediaPlayer.wmp.controls.Stop();
	}

	public static bool IsExisted(string path) {
		return System.IO.File.Exists(path);
	}
}

///////////////////////////////////////////////////////////////////////////////
}
