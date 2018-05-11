using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGUI : MonoBehaviour {

	public float progress = 0.0f;
	public Transform CanvasParent;
	List<GameObject> objList = new List<GameObject>();


	void OnGUI()
	{
		progress = AssetManagerSystem.AssetManager.I.GetProgress();
	}

	public void LoadManifest()
	{
		AssetManagerSystem.AssetManager.I.LoadManifest();
	}

	public void LoadAssetBundle()
	{
		AssetManagerSystem.AssetManager.I.LoadAssetAsync<GameObject>("Dialog","externalresources/prefab", (GameObject obj) =>{
			var go = Instantiate(obj) as GameObject;
			go.transform.SetParent( CanvasParent );
			go.transform.localPosition = Vector3.zero + UnityEngine.Random.Range( -3.0f, 3.0f) * 128f* ( Vector3.right + Vector3.up);
			objList.Add( go );
		});
	}


	public void LoadBGM()
	{
		AssetManagerSystem.AssetManager.I.LoadAssetAsync<AudioClip>("bgm_maoudamashii_8bit29","externalresources/sound/bgm", (AudioClip clip) =>
		{
			var go = new GameObject();
			var audioSource = go.AddComponent<AudioSource>();
			audioSource.clip = clip;
			audioSource.Play();

		});
	}


	public void ClearCache()
	{
		bool ret =AssetManagerSystem.AssetManager.I.ClearAllAssetCache();
		Debug.Log(ret ? "Cache clear succeeded" : "failed clear cache");
	}

	public void ClearList()
	{
		AssetManagerSystem.AssetManager.I.ForceClearList();
	}

	public void UnloadAsset()
	{
		if( objList.Count < 1)
		{
			return;
		}
		var go = objList[0];
		objList.RemoveAt( 0 );
		AssetManagerSystem.AssetManager.I.UnloadAsset( go );
		Destroy( go );
	}

	public void LogCacheAll()
	{
		var list = new List<string>();
		Caching.GetAllCachePaths( list);
		for (int i = 0, length=list.Count; i < length; i++)
		{
			Debug.Log(list[i]);
		}

		Debug.Log("TempCachePath:"+Application.temporaryCachePath);
	}

	public void DebugIsCached()
	{
		Debug.Log("IsCached ? "+ AssetManagerSystem.AssetManager.I.IsCached("externalresources/prefab") );
	}

}
