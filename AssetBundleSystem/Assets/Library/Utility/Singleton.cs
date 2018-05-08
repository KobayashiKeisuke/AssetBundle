using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetManagerSystem
{
	/// <summary>
	/// SIngleton クラス
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Singleton<T> : MonoBehaviour where T : Singleton<T>
	{
		//--------------------------------------------
		// メンバ変数
		//--------------------------------------------
		#region ===== MEMBER_VARIABLES =====

		private static T m_instance = null;
		public static T Instance{get{return m_instance;}}
		public static T I{get{return m_instance;}}
		
		#endregion //) ===== MEMBER_VARIABLES =====

		//--------------------------------------------
		// 初期化
		//--------------------------------------------
		#region ===== INIT =====

		/// <summary>
		/// 絶対に継承させない
		/// Singleton パターンが壊れる(可能性がある)
		/// </summary>
		private void Awake()
		{
			if( IsExist() )
			{
				return;
			}

			m_instance = this as T;
			Init();
		}

		/// <summary>
		/// 初期化処理
		/// 継承先で何らかの初期化処理が必要な場合はこちらへ
		/// ※Awake 以外からの呼び出し禁止
		/// </summary>
		protected virtual void Init()
		{

		}

		#endregion //) ===== INIT =====

		/// <summary>
		/// Singleton が存在するかどうか
		/// </summary>
		/// <returns></returns>
		public static bool IsExist(){return ( I != null ); }
	}

}
