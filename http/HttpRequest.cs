/*!
 *	@remark
 *		このプロジェクトは .Net Standard 2.0 / C# 7 をターゲットに作成。
 *		Unity のマルチプラットフォーム向け DLL のターゲットフレームワークに合わせることで、
 *		DLL に組み込み Unity で利用することを想定しているため。
 *		従って、上記のバージョンに合わないコードに修正することは禁止。
 *		null 許容型, new() などが使えない。詳細は C# 7 のドキュメントや書籍参照。
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Http;

using ResponseTask = System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage>;

///////////////////////////////////////////////////////////////////////////////////////////////////
namespace Dead { namespace Http {
///////////////////////////////////////////////////////////////////////////////////////////////////

/*!
 *	@class SimpleRequest
 *	@brief 簡易な HTTP 通信を行うクラス@n
 *		1. １つのアプリで通信するクライアントは１つ。@n
 *		2. ３分以内に通信が終わる程度のデータ量。@n
 *		3. 通信に失敗した場合、再接続する必要がない。@n
 *		4. 通信が終了するまで接続先 URL は同じ。@n
 *		5. サーバーに送信するデータは json 形式。@n
 *	以上の条件を満たす場合に使用できる。
 *
 * @see https://learn.microsoft.com/ja-jp/dotnet/fundamentals/networking/http/httpclient
 * @see https://learn.microsoft.com/ja-jp/dotnet/fundamentals/networking/http/httpclient-guidelines
 * @see https://oita.oika.me/2017/10/22/post-json-with-httpclient/
 */

public class SimpleRequest {
	readonly string         accessURL;
	//CancellationTokenSource canceller;

	readonly Dictionary<string, string> headers = new Dictionary<string, string>();

	//public bool IsCanceled { get; set; } = false;

	public SimpleRequest(string url) {
		if (string.IsNullOrEmpty(url)) { throw new ArgumentNullException("url"); }
		this.accessURL = url;
	}
	public void AddHeader(string name, string value) {
		this.headers.Add(name, value);
	}
	public void ClearHeaders() {
		this.headers.Clear();
	}
	public ResponseTask Post(StringContent content) {
		var client     = new HttpClient();
		//this.canceller = new CancellationTokenSource();

		var request = new HttpRequestMessage(HttpMethod.Post, this.accessURL);
		foreach (KeyValuePair<string, string> header in this.headers) {
			request.Headers.Add(header.Key, header.Value);
		}
		this.ClearHeaders();

		request.Content = content;

		return client.SendAsync(request);
	}
	//public void Cancel() {
	//	this.canceller?.Cancel();
	//	this.IsCanceled = true;
	//}
}

///////////////////////////////////////////////////////////////////////////////////////////////////
}}
///////////////////////////////////////////////////////////////////////////////////////////////////
