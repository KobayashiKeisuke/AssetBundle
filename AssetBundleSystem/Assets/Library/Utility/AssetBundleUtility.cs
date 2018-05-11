using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace AssetManagerSystem
{
	
	/// <summary>
	/// Utility メソッド定義用クラス
	/// </summary>
	public static class AssetBundleUtility
	{
		/// <summary>
		/// AssetBundle の保存Pathを生成
		/// </summary>
		/// <param name="_assetBundleName"></param>
		/// <returns></returns>
		public static string GetAssetBundlePath( string _assetBundleName )
		{
			return Path.Combine( Application.streamingAssetsPath, _assetBundleName);
		}
	}
}


