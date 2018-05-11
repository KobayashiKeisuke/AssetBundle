using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

using AssetManagerSystem;

namespace AssetManagerSystem.Editor
{
	public class AssetBundleBuild : UnityEditor.Editor
	{
		const string OUTPUT_BASE_PATH = "Assets/Build/AssetBundle/";

		[MenuItem("AssetBunedle/Tool/BuildAsset")]
		public static void BuildAllAsset()
		{
			string outputPath = Path.Combine(OUTPUT_BASE_PATH, AssetBundleUtility.GetPlatformName() );
			// Dir が無ければ作る
	        if (!Directory.Exists(outputPath) )
			{
				Directory.CreateDirectory (outputPath);
			}
			Debug.Log("Build Asset @"+outputPath);
            BuildPipeline.BuildAssetBundles (outputPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
		}


		[MenuItem("AssetBunedle/Tool/OpenCacheDir")]
		public static void OpenCacheDir()
		{
			string path = Caching.currentCacheForWriting.path;
			Debug.LogWarning(path);
			Debug.LogWarning(Application.persistentDataPath);
		}
	}
}