using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AssetManagerSystem
{
	/// <summary>
	/// AsssetBundle 全般を制御するためのSingleton クラス
	/// AssetBundleManager だとUnity公式のpackage
	/// ResourceManager だと2018.2 からの機能とぶつかるのでこの命名
	/// 
	/// Asset 作成はBuildScript 
	/// </summary>
	/// <typeparam name="AssetManager"></typeparam>
	public class AssetManager : Singleton<AssetManager>
	{
		//--------------------------------------------
		// 定数関連
		//--------------------------------------------
		#region ===== CONSTS =====

		const int DRFAULT_LOADED_ASSET_LIST_SIZE = 128;	// 同時に128個以上AssetをLoadする場合は要注意

		/// <summary>
		/// AssetBundleManifest関連の情報をまとめたクラス
		/// </summary>
		public class ManifestInfo
		{
			public AssetBundleManifest BundleManifest = null;
			public int Version = -1;

			/// <summary>
			/// 更新が必要かどうかの判定
			/// </summary>
			/// <param name="_version">比較したいManifestのVersion</param>
			/// <returns></returns>
			public bool IsRequiredUpdate( int _version){return _version > Version;}

			public bool HasManifest(){return BundleManifest != null;}
		}

		#endregion //) ===== CONSTS =====



		//--------------------------------------------
		// メンバ変数
		//--------------------------------------------
		#region ===== MEMBER_VARIABLES =====

		/* Manifest 関連 */
		ManifestInfo　m_manifestInfo = null;
		public ManifestInfo Manifest{get{return m_manifestInfo;}}
		
		private bool m_isDownloadManifest = false;


		// Load済Asset
		private Dictionary<string, AssetData> m_loadedAssetDict = new Dictionary<string, AssetData>(DRFAULT_LOADED_ASSET_LIST_SIZE);
		public Dictionary<string, AssetData> LoadedAssetDict{get{return m_loadedAssetDict;}}


		/* Load 管理 */
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

		/// <summary>
		/// 初期化処理
		/// Awake 以外からの呼び出し禁止
		/// </summary>
		protected override void Init()
		{
			// AssetLoad Coroutine などを回すためDestroy対象外
			DontDestroyOnLoad( this.gameObject );
		}

		#endregion //) ===== INIT =====


		//--------------------------------------------
		// API
		//--------------------------------------------
		#region ===== PUBLIC_API =====

		//--------------------------------------------
		// マニフェスト関連
		//--------------------------------------------
		#region ===== MANIFEST_PUBLIC_METHOD =====

		/// <summary>
		/// ManifestのValidation
		/// </summary>
		/// <returns></returns>
		public bool IsValidManifest(){ return (Manifest != null && Manifest.HasManifest() ); }

		/// <summary>
		/// Manifest のLoadリクエスト
		/// </summary>
		/// <param name="_manifestVersion">バージョン</param>
		public void LoadManifest( int _manifestVersion )
		{
			if( IsValidManifest() )
			{
				// 古いマニフェストは読み込ませない
				if( Manifest.Version > _manifestVersion )
				{
					return;
				}
			}
			StartCoroutine( DoLoadManifest() );
		}

		#endregion //) ===== MANIFEST_PUBLIC_METHOD =====	

		//--------------------------------------------
		// AssetBundle Load 
		//--------------------------------------------
		#region ===== LOAD_ASSET_BUNDLE_PUBLIC_METHOD =====

		/// <summary>
		/// 指定のAssetBundleがLoad済みかどうかチェック
		/// </summary>
		/// <param name="_assetBundleName"></param>
		/// <returns></returns>
		public bool IsLoadedAssetBundle( string _assetBundleName )
		{
			foreach( KeyValuePair<string, AssetData> pair in LoadedAssetDict )
			{
				if( pair.Value == null )
				{
					continue;
				}
				if( pair.Value.AssetName == _assetBundleName )
				{
					return true;
				}
			}
			return false;
		}



		/// <summary>
		/// AssetBundle のロードリクエスト
		/// </summary>
		/// <param name="_assetName"></param>
		public void LoadAssetBundle( string _assetName )
		{
			if( string.IsNullOrEmpty( _assetName))
			{
				return;
			}
			LoadAssetBundle( new string[]{_assetName});
		}
		/// <summary>
		/// AssetBundle のロードリクエスト
		/// 
		/// - メモリにLoad済み-> Load処理(ReferenceCount を上げる)
		/// - 未Loadだがローカルキャッシュにある -> Load して終了
		/// - キャッシュにもない -> Download -> Cache 保存 -> Load
		/// </summary>
		/// <param name="assetNames"></param>
		public void LoadAssetBundle(string[] _assetNames)
		{
			// Validation
			if( !IsValidManifest() || _assetNames == null || _assetNames.Length < 1)
			{
				return;
			}
		}
		#endregion //) ===== LOAD_ASSET_BUNDLE_PUBLIC_METHOD =====

		//--------------------------------------------
		// Download AssetBundle
		//--------------------------------------------
		#region ===== DOWNLOAD_ASSET_BUNDLE_PUBLIC_METHOD =====

		/// <summary>
		/// Donwload 中?
		/// </summary>
		/// <returns></returns>
		public bool IsDownloading(){ return TotalReqCount > 0; }

		/// <summary>
		/// 現在のDownload の進行度を取得
		/// </summary>
		/// <returns>Download中:[0.0 1.0], 停止中: -1.0f </returns>
		public float GetCurrentDownloadProgress()
		{
			if( !IsDownloading() )
			{
				return -1.0f;
			}
			float progress = 0.0f;
			for (int i = 0, length=m_currentDownloadRequestList.Count; i < length; i++)
			{
				progress += m_currentDownloadRequestList[i].downloadProgress;
			}

			return progress / (float)TotalReqCount;
		}

		/// <summary>
		/// AssetBundle のDownload
		/// </summary>
		/// <param name="_assetName"></param>
		public void DonwloadAssetBundle( string _assetName )
		{
			DonwloadAssetBundle( new string[]{ _assetName });
		}

		/// <summary>
		/// AssetBundle のDownload ~ Cacheに保存までを担当
		/// </summary>
		/// <param name="_assetNames"></param>
		public void DonwloadAssetBundle( string[] _assetNames )
		{

		}
		#endregion //) ===== DOWNLOAD_ASSET_BUNDLE_PUBLIC_METHOD =====

		//--------------------------------------------
		// Cache 関連
		//--------------------------------------------
		#region ===== CACHE =====

		/// <summary>
		/// 全Cache dir に存在するfileのpath を取得
		/// </summary>
		/// <returns></returns>
		public List<string> GetAllCachedAssetPaths()
		{
			List<string> paths = new List<string>();
			if( !Caching.ready )
			{
				return paths;
			}

			Caching.GetAllCachePaths(paths);

			return paths;
		}

		/// <summary>
		/// キャッシュ上に存在するかチェック
		/// </summary>
		/// <param name="_assetBundleName"></param>
		/// <returns></returns>
		public bool IsAssetBundleExistAtCache( string _assetBundleName )
		{
			List<string> paths = GetAllCachedAssetPaths();
			for (int i = 0, length=paths.Count; i < length; i++)
			{
				if( paths[i].Contains( _assetBundleName ))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 全キャッシュの削除
		/// </summary>
		/// <returns>何か利用中だったりCacheが使えない状態ならfalse</returns>
		public bool ClearAllCache()
		{
			if( !Caching.ready )
			{
				return false;
			}

			return Caching.ClearCache();
		}

		/// <summary>
		/// 指定のAssetBundle をキャッシュから削除
		/// </summary>
		/// <param name="_assetName">対象のAssetBundle名</param>
		/// <returns></returns>
		public bool ClearTargetAssetBundleCache( string _assetName)
		{
			if( !Caching.ready )
			{
				return false;
			}
			if( string.IsNullOrEmpty(_assetName) || !IsValidManifest() )
			{
				return false;
			}

			return Caching.ClearCachedVersion( _assetName, Manifest.BundleManifest.GetAssetBundleHash(_assetName) );
		}

		#endregion //) ===== CACHE =====

		#endregion //) ===== PUBLIC_API =====


		//--------------------------------------------
		// マニフェスト関連（ private )
		//--------------------------------------------
		#region ===== MANIFEST_PRIVATE_METHOD =====

		/// <summary>
		/// 実際のAssetBundleManifest Download 実行場所
		/// </summary>
		/// <returns></returns>
		IEnumerator DoLoadManifest()
		{
			while( m_isDownloadManifest )
			{
				yield return null;
			}

			m_isDownloadManifest = true;
			

        	// string url = RemoteAssetFilePath(_platform);
			string url = string.Empty;
			OnBeginDownload();
        	UnityWebRequest www = UnityWebRequest.Get(url);
        	www.downloadHandler = new AssetBundleDownloadHandler( OnCompleteDownload );
			
			// 登録
			m_currentDownloadRequestList.Add( www );

        	yield return www.SendWebRequest();
        	if (www.isNetworkError || www.isHttpError)
        	{
        	    Debug.LogError("manifest load failed. error: " + www.error);
        	    Debug.LogError("url: " + url);
        	}
			yield return null;
			// 登録解除
			m_currentDownloadRequestList.Remove( www );

			m_isDownloadManifest = false;
		}
		#endregion //) ===== MANIFEST_PRIVATE_METHOD =====	


		//--------------------------------------------
		// AssetBundle Load ( private )
		//--------------------------------------------
		#region ===== LOAD_ASSET_BUNDLE_PRIVATE_METHOD =====

		private IEnumerator DoLoadAssetBundle( string[] _assetNames )
		{
			yield return null;
		}
		#endregion //) ===== LOAD_ASSET_BUNDLE_PRIVATE_METHOD =====



		//--------------------------------------------
		// Download AssetBundle ( private )
		//--------------------------------------------
		#region ===== DOWNLOAD_ASSET_BUNDLE_PRIVATE_METHOD =====

		private IEnumerator DoDownloadAssetBundle( string[] _assetNames )
		{
			yield return null;
		}

		/// <summary>
		/// Download 開始時処理
		/// </summary>
		private void OnBeginDownload()
		{
			++m_totalRequestCount;
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
	

		//--------------------------------------------
		// Editor向けDebug機能
		//--------------------------------------------	
		#if UNITY_EDITOR
		#endif //) UNITY_EDITOR
	}
}

