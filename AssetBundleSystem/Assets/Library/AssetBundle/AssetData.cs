using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetManagerSystem
{
	/// <summary>
	/// Load済みAssetBundle に関する情報まとめクラス
	/// </summary>
	public class AssetData : IDisposable
	{
		//--------------------------------------------
		// メンバ変数
		//--------------------------------------------
		#region ===== MEMBER_VARIABLES =====

		// AssetBundle名
		string m_assetName = "";
		public string AssetName{get{return m_assetName;}}

		// ロード対象のAssetBundle
		private AssetBundle m_targetBundle = null;
		public AssetBundle TargetBundle{get{return m_targetBundle;}}

		// 参照カウンタ
		private int m_refCount = 0;
		public	int RefCount{get{return m_refCount;}}

		// このBundle のhash 値
		private Hash128 m_assetHash;
		public	Hash128 AssetHash{get{return m_assetHash;}}

		#endregion //) ===== MEMBER_VARIABLES =====



		//--------------------------------------------
		// 初期化
		//--------------------------------------------
		#region ===== INIT =====

		public AssetData()
		{
			m_refCount = 0;
		}

		public AssetData( AssetBundle _loadedBundle, Hash128 _hash ) : this()
		{
			m_targetBundle = _loadedBundle;

			m_assetHash = _hash;
		}

		#endregion //) ===== INIT =====

		//--------------------------------------------
		// Dispose
		//--------------------------------------------
		#region ===== DISPOSE =====

		public void Dispose( )
		{

		}

		#endregion //) ===== DISPOSE =====


		//--------------------------------------------
		// 参照カウンタ
		//--------------------------------------------
		#region ===== REFERENCE_COUNT =====

		public void SetReference( )
		{
			++m_refCount;
		}

		public void SetRelease()
		{
			--m_refCount;
			if( RefCount < 1 )
			{
				//Unload
			}
		}

		#endregion //) ===== REFERENCE_COUNT =====
	}

}
