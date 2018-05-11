using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


namespace AssetManagerSystem
{
	/// <summary>
	/// AssetBundle のDownload 周りを制御するAPI提供クラス
	/// 外部(主にAssetManager )にコルーチンを代理実行してもらってください
	/// </summary>
	public class AssetBundleDonwloadController
	{
		//--------------------------------------------
		// 定数関連
		//--------------------------------------------
		#region ===== CONSTS =====

		public const float INVALID_DONWLOAD_PROGRESS = -1.0f;		// Download してなかったり異常がある場合のProgress 値


		/* コールバック定義 */
		public delegate void OnCompleteDownloadEachBundle( bool _isSucceeded, AssetBundle _bundle);				// AssetBundle 1個ずつDLが終わるたびに呼ぶコールバック
		public delegate void OnCompleteDownloadAllBundle( int _isSucceededCount, AssetBundle[] _bundle);		// AssetBundle 全部がDL終了したときのコールバック
		
		#endregion //) ===== CONSTS =====
		
		//--------------------------------------------
		// メンバ変数
		//--------------------------------------------
		#region ===== MEMBER_VARIABLES =====

		/* 実行中リクエスト 管理 */
		List<UnityWebRequest> m_currentDownloadRequestList = new List<UnityWebRequest>();	// 現在実行中のRequest

		private int m_totalRequestCount = 0;		// 現在実行しているRequest 数
		public	int TotalReqCount{get{return m_totalRequestCount;}}

		private int m_totalCompRequestCount = 0;	// 現在完了しているRequest 数
		public	int TotalCompReqCount{get{return m_totalCompRequestCount;}}
		#endregion //) ===== MEMBER_VARIABLES =====

		//--------------------------------------------
		// 初期化
		//--------------------------------------------
		#region ===== INIT =====

		public AssetBundleDonwloadController()
		{
			m_currentDownloadRequestList.Clear();
			m_totalCompRequestCount = 0;
			m_totalRequestCount = 0;
		}

		#endregion //) ===== INIT =====

		//--------------------------------------------
		// Donwload エンジン
		//--------------------------------------------
		#region ===== DONWLOAD_ENGINE =====

		/// <summary>
		/// 指定のAssetBundle のDownload 処理
		/// </summary>
		/// <param name="_assetBundleName">AssetBundle 名</param>
		/// <param name="_onComplete"></param>
		/// <returns></returns>
		public IEnumerator DoDownloadAssetBundle( string _assetBundleName, OnCompleteDownloadEachBundle _onComplete )
		{
			// DL 対象が無い
			if( string.IsNullOrEmpty( _assetBundleName ))
			{
				_onComplete.Invoke( false, null);
				yield break;
			}

			// TODO: 複数AssetDownload 対応
			OnBeginDownload( 1 );

			string uri = string.Empty;
			UnityWebRequest webRequest = UnityWebRequest.GetAssetBundle( uri:uri );
			webRequest.downloadHandler = new AssetBundleDownloadHandler( OnCompleteDownload );
			// 登録
			m_currentDownloadRequestList.Add( webRequest );

			// DL 開始 & 終了待機
			yield return webRequest.SendWebRequest();

			// 登録解除
			m_currentDownloadRequestList.Remove( webRequest );

			bool _isSucceeded = ( ! webRequest.isHttpError && !webRequest.isNetworkError);
			if( _onComplete != null )
			{
				_onComplete.Invoke( _isSucceeded, DownloadHandlerAssetBundle.GetContent(webRequest));
			}

		}
	
		#endregion //) ===== DONWLOAD_ENGINE =====


		//--------------------------------------------
		// API
		//--------------------------------------------
		#region ===== PUBLIC_API =====

		/// <summary>
		/// 現在のダウンロード進行状況を取得
		/// </summary>
		/// <returns>-1.0f : 以上終了 or 実行なし, 0.0~1.0:実行中(100倍して%化して使うと便利)</returns>
		public float GetCurrentProgress()
		{
			if( m_currentDownloadRequestList == null || m_totalRequestCount < 1)
			{
				return INVALID_DONWLOAD_PROGRESS;
			}

			float ratio = (float)(TotalReqCount - m_currentDownloadRequestList.Count)/(float)m_totalRequestCount;
			for (int i = 0, length=m_currentDownloadRequestList.Count; i < length; i++)
			{
				ratio += m_currentDownloadRequestList[i].downloadProgress / (float)m_totalRequestCount;
			}

			return Mathf.Clamp01( ratio );
		}
	
		#endregion //) ===== PUBLIC_API =====




		
		//--------------------------------------------
		// Download AssetBundle ( private )
		//--------------------------------------------
		#region ===== DOWNLOAD_ASSET_BUNDLE_PRIVATE_METHOD =====

		/// <summary>
		/// Download 開始時処理
		/// </summary>
		/// <param name="_reqCount"></param>
		private void OnBeginDownload(int _reqCount)
		{
			m_totalRequestCount += Mathf.Max(0, _reqCount);
		}

		/// <summary>
		/// AssetBundle Donwload 完了時コールバック
		/// </summary>
		private void OnCompleteDownload()
		{
			++m_totalCompRequestCount;
			// 全DL完了
			if( TotalReqCount == TotalCompReqCount)
			{
				m_totalRequestCount = 0;
				m_totalCompRequestCount = 0;
			}
		}
		#endregion //) ===== DOWNLOAD_ASSET_BUNDLE_PRIVATE_METHOD =====	
	
	}
}


