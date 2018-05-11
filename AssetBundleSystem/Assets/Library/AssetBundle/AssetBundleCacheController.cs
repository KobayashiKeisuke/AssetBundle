using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



namespace AssetManagerSystem
{
	/// <summary>
	/// AssetBundle の Cache管理を司るクラス
	/// </summary>
	public class AssetBundleCacheController 
	{
		//--------------------------------------------
		// 定数関連
		//--------------------------------------------
		#region ===== CONSTS =====

		/* コールバック関数定義 */
		public delegate void OnCompleteLoadFromCache( bool _isSucceeded, string _assetBundleName, AssetData _data );	// Cache からAssetBundle ロード完了時コールバック

		#endregion //) ===== CONSTS =====

		//--------------------------------------------
		// Cache制御
		//--------------------------------------------
		#region ===== CACHE =====

		/// <summary>
		/// 指定したAssetBundleをキャッシュから削除
		/// </summary>
		/// <param name="_assetName">Asset名</param>
		/// <returns></returns>
		public static bool ClearTargetAssetCache( string _assetName, AssetBundleManifest _manifest )
		{
			if( !Caching.ready )
			{
				return false;
			}
			if( string.IsNullOrEmpty( _assetName) || _manifest == null)
			{
				return false;
			}
			return Caching.ClearCachedVersion( _assetName, _manifest.GetAssetBundleHash(_assetName));
		}

		/// <summary>
		/// ローカルキャッシュから全てのAssetBundle を削除
		/// </summary>
		/// <returns></returns>
		public static bool ClearAllAssetCache( )
		{
			if( !Caching.ready )
			{
				return false;
			}

			return Caching.ClearCache();
		}

		/// <summary>
		/// 対象のAssetBundleがキャッシュ上に存在するかチェック
		/// </summary>
		/// <param name="_assetBundleURL">AssetBundle名ではないので注意</param>
		/// <param name="_hash">Hash128, int によるバージョン指定はobsolute になりました</param>
		/// <returns></returns>
		public static bool IsCached( string _assetBundleURL, Hash128 _hash )
		{
			if( !Caching.ready )
			{
				return false;
			}

			return Caching.IsVersionCached( _assetBundleURL, _hash);
		}

		#endregion //) ===== CACHE =====



		//--------------------------------------------
		// LOAD
		//--------------------------------------------

		#region ===== LOAD =====
		/// <summary>
		/// ローカルキャッシュからAssetBundle をロード
		/// IsCached で存在チェックしている前提
		/// </summary>
		/// <param name="_assetBundleName">AssetBundle名</param>
		/// <param name="_onComplete">処理終了時コールバック</param>
		/// <returns></returns>
		public static　IEnumerator LoadAssetBundleFromCache( string _assetBundleName, AssetBundleManifest _manifest, OnCompleteLoadFromCache _onComplete )
		{
			string path = AssetBundleUtility.GetAssetBundlePath( _assetBundleName );
			AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(path);

			// Load 完了まで待機
			yield return req;

			// Load成功していたら登録
			bool isSucceeded = ( req.assetBundle != null && _manifest != null );
			AssetData data = null;
			if( isSucceeded)
			{
				data = new AssetData( req.assetBundle, _manifest.GetAssetBundleHash(req.assetBundle.name) );
			}

			if( _onComplete != null )
			{
				_onComplete.Invoke( isSucceeded, _assetBundleName, data );
			}
		}

		#endregion //) ===== LOAD =====		

	}
}

