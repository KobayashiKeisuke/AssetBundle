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
		public delegate void OnCompleteDownloadAllBundle( int _isSucceededCount, string[] _failedList);			// AssetBundle 全部がDL終了したときのコールバック
		
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
		/// AssetBundle のDonwload
		/// </summary>
		/// <param name="_bundleNames">AssetBundle名(複数)</param>
		/// <param name="_onEachComplete">各AssetBundle のDL完了時コールバック</param>
		/// <param name="_onAllComplete">全AssetBudle のDL完了コールバック</param>
		/// <returns></returns>
		public IEnumerator DoDownloadAssetBundles( string[] _bundleNames, OnCompleteDownloadEachBundle _onEachComplete, OnCompleteDownloadAllBundle _onAllComplete)
		{
			Debug.LogWarning("Start Download");
			// DL 対象が無い
			if( _bundleNames == null || _bundleNames.Length < 1)
			{
				_onAllComplete.Invoke( 0, _bundleNames );
				yield break;
			}
			// Download 開始宣言
			OnBeginDownload( _bundleNames.Length );
			// 失敗したリストを用意
			List<string> failedList = new List<string>();

			//各アセットのDownload
			for (int i = 0; i < _bundleNames.Length; i++)
			{
				string uri = string.Format("{0}/{1}/{2}", AssetBundleUtility.HOST,AssetBundleUtility.GetPlatformName(),_bundleNames[i]);
				UnityWebRequest webRequest = UnityWebRequest.GetAssetBundle( uri:uri );
				// webRequest.downloadHandler = new AssetBundleDownloadHandler( OnCompleteDownload );
				// 登録
				m_currentDownloadRequestList.Add( webRequest );

				// DL 開始 & 終了待機
				yield return webRequest.SendWebRequest();

				OnCompleteDownload();
				
				// 登録解除
				m_currentDownloadRequestList.Remove( webRequest );

				bool isSucceeded = ( ! webRequest.isHttpError && !webRequest.isNetworkError);
				if( _onEachComplete != null )
				{
					_onEachComplete.Invoke( isSucceeded, DownloadHandlerAssetBundle.GetContent(webRequest));
				}

				if( !isSucceeded)
				{
					failedList.Add( _bundleNames[i]);
				}
			}

			Debug.LogWarning("End Download");


			// Complete 処理
			if( _onAllComplete != null )
			{
				_onAllComplete.Invoke( _bundleNames.Length - failedList.Count, failedList.ToArray() );
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


