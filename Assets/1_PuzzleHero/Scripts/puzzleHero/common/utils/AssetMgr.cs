
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/**
 * Generally load resources from Resources folder 
 * 
*/
using strange.examples.strangerocks;


public class AssetMgr
{
	public delegate void LoadAssetBundleComplete();
	public delegate void LoadProgress(float percent);

	public event LoadAssetBundleComplete CompleteHandlers;
	public event LoadProgress LoadProgressHandlers;

	private List<AssetBundle> assets = new List<AssetBundle>();
	private int curTotalLoad;
	private float curTotalProgress;
	private bool isDownloading = false;

	[Inject]
	public IRoutineRunner runner {get;set;}

	public AssetMgr ()
	{
	}

	public void AddAssetBundle(AssetBundle asset)
	{
		assets.Add(asset);
		if(curTotalLoad == assets.Count)
		{
			if(CompleteHandlers != null)
			{
				isDownloading = false;
				CompleteHandlers.Invoke();
			}
		}
	}

	public void GetAsset<T>(string name, System.Action<T> callback) where T : Object
	{
		runner.StartCoroutine(GetAssetRunner(name, callback));
	}

	public T GetAssetSync<T>(string name) where T : Object
	{
		if (!isDownloading) {
			Object obj = null;
			obj = Resources.Load<T>(name);
			if (obj == null) {
				System.Type type = typeof(T);
				int len = assets.Count;
				for (int i = 0; i < len; ++i) {
					obj = assets[i].Load(name, type);
					if (obj != null)
						break;
				}
			}
			return obj as T;
		}
		else {
			throw new System.Exception("prevent from getting content while downloading");
		}
	}

	public IEnumerator GetAssetRunner<T>(string name, System.Action<T> callback) where T : Object
	{
		if(!isDownloading)
		{
			Object obj = null;
			ResourceRequest req = Resources.LoadAsync<T>(name);
			yield return req;
			obj = req.asset;
			if(obj == null)
			{
				System.Type type = typeof(T);
				int len = assets.Count;
				for(int i = 0; i < len; ++i)
				{
					AssetBundleRequest bundlReq = assets[i].LoadAsync(name,type);
					yield return bundlReq;
					obj = bundlReq.asset;
					if(obj != null)
						break;
				}
			}
			callback((T)obj);
		}else
		{
			throw new System.Exception("prevent from getting content while downloading");
		}
	}

	public void GetComponentFromAsset<T>(string name, System.Action<T> callback) where T : Component
	{
		GetAsset<GameObject>(name,delegate(GameObject go) {
			if(go != null)
			{
				callback(go.GetComponent<T>());
			}else
			{
				callback(null);
			}
		});
	}

	public void DownloadAssetBundle(string url, int ver) 
	{
		runner.StartCoroutine(DownloadAssetBundleRunner(url,ver));
	}

	public IEnumerator DownloadAssetBundleRunner(string url, int ver)
	{
		++curTotalLoad;
		isDownloading = true;
		while (!Caching.ready)
			yield return null;
		using(WWW www = WWW.LoadFromCacheOrDownload (url, ver))
		{
			TrackingProgress(www);
			yield return www;
			TrackingProgress(www);
			if (www.error != null)
				throw new System.Exception("WWW download:" + www.error);
			AssetBundle assetBundle = www.assetBundle;
			AddAssetBundle(assetBundle);
			//assetBundle.Unload(false);
		}
	}

	private void TrackingProgress(WWW www) 
	{
		if(LoadProgressHandlers != null)
		{
			curTotalProgress += www.progress;
			LoadProgressHandlers.Invoke(curTotalProgress / curTotalLoad);
		}
	}

	public void Trace() {
		foreach(AssetBundle asset in assets)
		{
			Object[] objs = asset.LoadAll();
			foreach(Object obj in objs)
			{
				Debug.Log(obj.name + " : " + obj);
			}
		}
	}


}


