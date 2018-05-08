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
		// マニフェスト関連
		//--------------------------------------------
		#region ===== MANIFEST =====

		/// <summary>
		/// ManifestのValidation
		/// </summary>
		/// <returns></returns>
		public bool IsValidManifest(){ return (Manifest != null && Manifest.HasManifest() ); }

		public void LoadManifest()
		{
			StartCoroutine( DoLoadManifest() );
		}

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
			

			yield return null;


			m_isDownloadManifest = false;
		}
		#endregion //) ===== MANIFEST =====	

		//--------------------------------------------
		// AssetBundle Load
		//--------------------------------------------
		#region ===== LOAD_ASSET_BUNDLE =====

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

		private IEnumerator DoLoadAssetBundle( string[] _assetNames )
		{
			yield return null;
		}
		#endregion //) ===== LOAD_ASSET_BUNDLE =====

		//--------------------------------------------
		// Download AssetBundle 
		//--------------------------------------------
		#region ===== DOWNLOAD_ASSET_BUNDLE =====

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

		private IEnumerator DoDownloadAssetBundle( string[] _assetNames )
		{
			yield return null;
		}
		#endregion //) ===== DOWNLOAD_ASSET_BUNDLE =====	




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
	}
}

