using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetManagerSystem
{

	/// <summary>
	/// AssetBundle のロード制御クラス
	/// </summary>
	public class AssetBundleLoadController 
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

		/* コールバック関数定義 */
		public delegate void OnCompleteLoadAsset<T>( T _asset ) where T: UnityEngine.Object;		// AssetBundle からAsset ロード完了時コールバック

		#endregion //) ===== CONSTS =====

		//--------------------------------------------
		// メンバ変数
		//--------------------------------------------
		#region ===== MEMBER_VARIABLES =====

		// コルーチン代理実行者
		ICoroutineExecutor m_coroutineExecutor = null;

		// Download制御クラス
		AssetBundleDonwloadController m_downloadCtrl = null;
		
		/* Manifest 関連 */
		ManifestInfo　m_manifestInfo = null;
		public ManifestInfo Manifest{get{return m_manifestInfo;}}

		/* メモリにいるAsset情報 */
		// TODO: Dictionary は重いので独自Dictionary をあとで実装して変更
		private Dictionary<string, AssetData> m_loadedAssetList = new Dictionary<string, AssetData>();
		protected Dictionary<string, AssetData> LoadedAssetList{get{return m_loadedAssetList;}}
		#endregion //) ===== MEMBER_VARIABLES =====


		//--------------------------------------------
		// 初期化
		//--------------------------------------------
		#region ===== INIT =====

		public AssetBundleLoadController( ICoroutineExecutor _executor )
		{
			m_coroutineExecutor = _executor;
			m_downloadCtrl = new AssetBundleDonwloadController();
		}
		#endregion //) ===== INIT =====


		//--------------------------------------------
		// よく使うAPI群
		//--------------------------------------------
		#region ===== PUBLIC_API =====

		//--------------------------------------------
		// AssetBundle Load
		//--------------------------------------------
		#region ===== LOAD =====

		/// <summary>
		/// Load = Get
		/// </summary>
		/// <param name="_assetName"></param>
		/// <param name="_assetBundleName"></param>
		/// <param name="_onComplete"></param>
		/// <typeparam name="T"></typeparam>
		public void LoadAssetAsync<T>( string _assetName, string _assetBundleName, OnCompleteLoadAsset<T> _onComplete) where T : UnityEngine.Object
		{
			Debug.Assert( IsValidManifest(), " Manifest Does not exist" );
			Debug.Assert( m_coroutineExecutor != null, " Coroutine Executor Does not exist" );

			// Validation
			if( m_coroutineExecutor == null )
			{
				if( _onComplete != null )
				{
					_onComplete.Invoke( null );
				}
				return;
			}

			/* ------ Flow ------
			* 1. 指定のAssetBundle がLoad済みかチェック -> Asset を Load -> Callback
			* 2. ローカルキャッシュに存在する -> Cache からLoad -> Asset を Load -> Callback
			* 3. ローカルキャッシュにも存在しない -> Download & Cache に保存 -> Cache からLoad -> Asset を Load -> Callback
			*/

			// Load済みなのでそのままAsset をロードする
			if( IsLoadedAsset( _assetName, _assetBundleName))
			{
				m_coroutineExecutor.InvokeCoroutine( 
					LoadAsset<T>( _assetName, _assetBundleName, _onComplete )
				);
				return;
			}

			// AssetBundle 名の指定がないとキャッシュ検索やDownloadが実行できない
			// また、Manifest が無いと依存関係とかわからないのでチェック
			if( string.IsNullOrEmpty(_assetBundleName ) || !IsValidManifest() )
			{
				if( _onComplete != null )
				{
					_onComplete.Invoke( null );
				}
				return;
			}

			// Cache ロード完了コールバック
			AssetBundleCacheController.OnCompleteLoadFromCache OnCompleteCacheLoad = 
			( bool _isSucceeded, string _abName, AssetData _data ) => 
			{
				if( _isSucceeded )
				{
					// まずは成功データを登録
					bool ret = RegisterAssetData( _abName, _data );
					if( !ret && _onComplete != null  )
					{
						// 登録失敗なので打ち止め
						_onComplete.Invoke( null );
						return;
					}

					// 成功したので続いてLoad
					m_coroutineExecutor.InvokeCoroutine( 
						LoadAsset<T>( _assetName, _assetBundleName, _onComplete )
					);
				}
				else
				{
					Debug.LogError("Load Cache was Failed @"+_assetBundleName);
					if( _onComplete != null )
					{
						_onComplete.Invoke( null );
					}
				}
			};

			// Cacheにいるかチェック
			if( AssetBundleCacheController.IsCached( _assetBundleName, Manifest.BundleManifest.GetAssetBundleHash(_assetBundleName) )) 
			{
				m_coroutineExecutor.InvokeCoroutine( 
					AssetBundleCacheController.LoadAssetBundleFromCache( 
						_assetBundleName,
						Manifest.BundleManifest, 
						OnCompleteCacheLoad
					)
				);
				return;
			}

			// コントローラーがいないのでDownload 不可能
			if( m_downloadCtrl == null )
			{
				if( _onComplete != null )
				{
					_onComplete.Invoke( null );
				}
				return;
			}

			// Download から全て実行
			
			m_coroutineExecutor.InvokeCoroutine( 
				m_downloadCtrl.DoDownloadAssetBundle(_assetBundleName, (bool _isSucceeded , AssetBundle _ab )=>
				{
					// Download 失敗
					if( !_isSucceeded || _ab == null )
					{
						//打ち止め
						if( _onComplete != null )
						{
							_onComplete.Invoke( null );
						}
						return;
					}
					// あとはキャッシュロード完了後と同じ処理
					OnCompleteCacheLoad( _isSucceeded, _assetBundleName, new AssetData( _ab, Manifest.BundleManifest.GetAssetBundleHash(_assetBundleName) ) );
				})
			);
		}
		#endregion //) ===== LOAD =====

		//--------------------------------------------
		// ローカルキャッシュ管理
		//--------------------------------------------
		#region ===== CACHE =====

		/// <summary>
		/// 指定したAssetBundleをキャッシュから削除
		/// </summary>
		/// <param name="_assetName">Asset名</param>
		/// <returns></returns>
		public bool ClearTargetAssetCache( string _assetName )
		{
			//Validation
			if( !IsValidManifest() )
			{
				return false;
			}

			return AssetBundleCacheController.ClearTargetAssetCache( _assetName, Manifest.BundleManifest);
		}
		#endregion //) ===== CACHE =====

		#endregion //) ===== PUBLIC_API =====
	
	
	
		//--------------------------------------------
		// Manifest 関連
		//--------------------------------------------
		#region ===== MANIFEST =====

		/// <summary>
		/// ManifestのValidation
		/// </summary>
		/// <returns></returns>
		private bool IsValidManifest(){ return Manifest != null && Manifest.HasManifest(); }

		/// <summary>
		/// Manifestの更新が必要かどうか
		/// </summary>
		/// <param name="_version"></param>
		/// <returns></returns>
		private bool IsRequiredUpdate(int _version ){ return Manifest != null ? Manifest.IsRequiredUpdate(_version) : true; }

		#endregion //) ===== MANIFEST =====

		//--------------------------------------------
		// LoadedAssetBundle 関連
		//--------------------------------------------
		#region ===== LOADED_ASSET_BUNDLE =====


		/// <summary>
		/// 対象のAssetがロード済みAssetBundle 内に存在するかチェック
		/// </summary>
		/// <param name="_assetName">Asset名</param>
		/// <param name="_assetBundleName">AssetBundle名. 指定した方が検索が早い</param>
		/// <returns></returns>
		protected bool IsLoadedAsset( string _assetName, string _assetBundleName )
		{
			return ( GetAssetData(_assetName, _assetBundleName ) != null);
		}

		/// <summary>
		/// Load済みリストからAssetBundle データを取得
		/// </summary>
		/// <param name="_assetName"></param>
		/// <param name="_assetBundleName"></param>
		/// <returns>存在すればAssetData, なければnull</returns>
		protected AssetData GetAssetData( string _assetName, string _assetBundleName )
		{
			// AssetBundle 名指定があれば、それで検索
			if( !string.IsNullOrEmpty(_assetBundleName))
			{
				AssetData data = null;
				LoadedAssetList.TryGetValue( _assetBundleName, out data);

				return data;
			}

			// AssetBundle 指定なしなら全探索
			foreach( KeyValuePair<string, AssetData> pair in LoadedAssetList )
			{
				if( pair.Value.IsContainsAsset(_assetName))
				{
					return pair.Value;
				}
			}

			return null;			
		}

		/// <summary>
		/// AssetBundle をLoadedList に登録
		/// </summary>
		/// <param name="_assetBundleName">AssetBundle名(key)</param>
		/// <param name="_data">紐づけるデータ(Value)</param>
		/// <returns></returns>
		protected bool RegisterAssetData( string _assetBundleName, AssetData _data )
		{
			//Validation
			if( string.IsNullOrEmpty(_assetBundleName) || _data == null )
			{
				return false;
			}

			LoadedAssetList.Add( _assetBundleName, _data );
			return true;
		}

		/// <summary>
		/// 指定のAssetをLoad済みAssetBundle から取得
		/// 基本的にはIsLoadedAsset で
		/// </summary>
		/// <param name="_assetName"></param>
		/// <param name="_assetBundleName"></param>
		/// <param name="_onComplete"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		protected IEnumerator LoadAsset<T>(string _assetName, string _assetBundleName, OnCompleteLoadAsset<T> _onComplete ) where T : UnityEngine.Object
		{
			AssetData data = GetAssetData(_assetName, _assetBundleName);
			if( data == null )
			{
				if( _onComplete != null )
				{
					_onComplete.Invoke( null );
				}
				yield break;
			}

			AssetBundleRequest req = data.TargetBundle.LoadAssetAsync( _assetName);

			// Wait
			yield return req;

			if( req.asset != null )
			{
				// Load 成功なので参照カウントを増やす
				data.SetReference();
				if( _onComplete != null )
				{
					_onComplete.Invoke( req.asset as T );
				}
			}
			else
			{
				if( _onComplete != null )
				{
					_onComplete.Invoke( null );
				}
			}
		}

		#endregion ===== LOADED_ASSET_BUNDLE =====
			


		//--------------------------------------------
		// Utility 
		//--------------------------------------------
		#region ===== UITLITY =====

		/// <summary>
		/// 入力値のAsset名配列のバリデーション
		/// </summary>
		/// <param name="_assetNames"></param>
		/// <returns></returns>
		private string[] ValidateAssetNameList( string[] _assetNames )
		{
			if( _assetNames == null || _assetNames.Length < 1)
			{
				Debug.Assert( _assetNames != null, "Argument is NULL!!!!");
				if( _assetNames != null )
				{
					Debug.Assert( _assetNames.Length > 0, "name list is EMPTY!!!!");
				}

				return null;
			}

			List<string> names = new List<string>();
			for (int i = 0; i < _assetNames.Length; i++)
			{
				if( !string.IsNullOrEmpty(_assetNames[i]))
				{
					names.Add( _assetNames[i]);
				}
			}

			return names.ToArray();
		}


		#endregion //) ===== UITLITY =====		
	}
}

