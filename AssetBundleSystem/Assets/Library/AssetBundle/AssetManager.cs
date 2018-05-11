using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AssetManagerSystem
{
	/// <summary>
	/// AsssetBundle 全般を制御するためのSingleton クラス 兼ファサード
	/// AssetBundleManager だとUnity公式のpackage
	/// ResourceManager だと2018.2 からの機能とぶつかるのでこの命名
	/// 
	/// Asset 作成はBuildScript 
	/// </summary>
	/// <typeparam name="AssetManager"></typeparam>
	public class AssetManager : Singleton<AssetManager>, ICoroutineExecutor
	{
		//--------------------------------------------
		// メンバ変数
		//--------------------------------------------
		#region ===== MEMBER_VARIABLES =====

		// メモリ管理/Cache管理専任
		AssetBundleLoadController m_loadCtrl = null;

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
			m_loadCtrl = new AssetBundleLoadController( this );

			// AssetLoad Coroutine などを回すためDestroy対象外
			DontDestroyOnLoad( this.gameObject );
		}

		#endregion //) ===== INIT =====


		//--------------------------------------------
		// API
		//--------------------------------------------
		#region ===== PUBLIC_API =====

		/// <summary>
		/// AssetBundleManifest のロード
		/// これを初めに行わないと始まらない
		/// </summary>
		public void LoadManifest(){}

		/// <summary>
		/// Asset の取得
		/// Download の有無に関わらず取得するときはこれを叩いてください
		/// </summary>
		/// <param name="_assetName">対象のAsset名</param>
		/// <param name="_assetBundleName">対象のAssetを含むAssetBundle名. Empty でもいいですが、検索速度が落ちたり、Download が走らなかったりします</param>
		/// <param name="_onComplete">Load完了時コールバック</param>
		/// <typeparam name="T"></typeparam>
		public void LoadAssetAsync<T>( string _assetName, string _assetBundleName, AssetBundleLoadController.OnCompleteLoadAsset<T> _onComplete) where T : UnityEngine.Object
		{
			// Assertion
			Debug.Assert( !string.IsNullOrEmpty(_assetName), string.Format("Empty Asset Name ( AssetBundleName:{0}", _assetBundleName) );
			Debug.Assert( m_loadCtrl != null, "Load Controller is NULL!! Initialize was not Done!!" );

			// Validation
			if( string.IsNullOrEmpty(_assetName) || m_loadCtrl == null)
			{
				if( _onComplete != null )
				{
					_onComplete.Invoke( null );
				}
				return;
			}

			m_loadCtrl.LoadAssetAsync<T>( _assetName, _assetBundleName, _onComplete);
		}

		/// <summary>
		/// 指定のAssetBundle をUnload する
		/// 具体的にはLoadController が実行
		/// </summary>
		/// <param name="_target"></param>
		public void UnloadAsset(Object _target){}


		//--------------------------------------------
		// キャッシュ周り
		//--------------------------------------------
		#region ===== CACHE =====

		/// <summary>
		/// 指定したAssetBundleをキャッシュから削除
		/// </summary>
		/// <param name="_assetName">Asset名</param>
		/// <returns>true:成功, false: 異常終了orどこかでまだ参照を持っている</returns>
		public bool ClearTargetAssetCache( string _assetName ){ return m_loadCtrl == null ? false : m_loadCtrl.ClearTargetAssetCache(_assetName); }

		/// <summary>
		/// ローカルキャッシュから全てのAssetBundle を削除
		/// </summary>
		/// <returns>true:成功, false: 異常終了orどこかでまだ参照を持っているものが1つ以上残っている</returns>
		public bool ClearAllAssetCache( ){ return AssetBundleCacheController.ClearAllAssetCache(); }

		#endregion //) ===== CACHE =====


		#endregion //)  ===== PUBLIC_API =====


		//--------------------------------------------
		// CoroutineExecutor
		//--------------------------------------------
		#region ===== COROUTINE_EXECUTOR =====

		public void InvokeCoroutine( IEnumerator _coroutine)
		{
			if( _coroutine == null )
			{
				return;
			}

			StartCoroutine( _coroutine );
		}
		#endregion //) ===== COROUTINE_EXECUTOR =====


		//--------------------------------------------
		// Editor向けDebug機能
		//--------------------------------------------	
		#if UNITY_EDITOR
		#endif //) UNITY_EDITOR
	}
}

