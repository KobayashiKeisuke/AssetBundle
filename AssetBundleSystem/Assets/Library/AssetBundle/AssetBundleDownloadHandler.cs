using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AssetManagerSystem
{
	/// <summary>
	/// Download 中の処理などを管理する為のクラス
	/// </summary>
	public class AssetBundleDownloadHandler : DownloadHandlerScript
	{
		//--------------------------------------------
		//  メンバ変数
		//--------------------------------------------
		#region ===== MEMBER_VARIABLES =====

		// Callback
		System.Action m_onCompleteCallback = null;

		#endregion //) ===== MEMBER_VARIABLES =====

		//--------------------------------------------
		//  初期化メソッド
		//--------------------------------------------
		#region ===== INIT =====

		public AssetBundleDownloadHandler(){}

		public AssetBundleDownloadHandler( System.Action _onCompleteCallback )
		{
			m_onCompleteCallback = _onCompleteCallback;
		}
		#endregion //) ===== INIT =====

		//--------------------------------------------
		//  override method 
		//--------------------------------------------
		#region ===== OVERRIDE_METOHD =====

		/// <summary>
		/// Donwload 完了時コールバック
		/// </summary>
		protected override void CompleteContent()
		{
			if( m_onCompleteCallback != null)
			{
				m_onCompleteCallback.Invoke();
			}
		}

		#endregion //) ===== OVERRIDE_METOHD =====
	}
}
