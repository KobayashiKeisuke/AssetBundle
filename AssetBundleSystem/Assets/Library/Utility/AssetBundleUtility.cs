﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace AssetManagerSystem
{
	
	/// <summary>
	/// Utility メソッド定義用クラス
	/// </summary>
	public static class AssetBundleUtility
	{
		public const string HOST = "file:///users/keisuke.kobayashi/Desktop/AssetBundle";
		/// <summary>
		/// AssetBundle の保存Pathを生成
		/// </summary>
		/// <param name="_assetBundleName"></param>
		/// <returns></returns>
		public static string GetAssetBundlePath( string _assetBundleName )
		{
			return Path.Combine( Application.streamingAssetsPath, _assetBundleName);
		}





		public static string GetPlatformName()
        {
			#if UNITY_EDITOR
            return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
			#else
            return GetPlatformForAssetBundles(Application.platform);
			#endif
        }

		#if UNITY_EDITOR
        private static string GetPlatformForAssetBundles(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
                default:
                    return null;
            }
        }
		#endif

        private static string GetPlatformForAssetBundles(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                    return "OSX";
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
                default:
                    return null;
            }
        }
	}


	
}


