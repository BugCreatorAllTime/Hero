using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class DungeonAssetsRegister
{
	public delegate void OnAssetLoadedDelegate();

	public delegate void OnAllAssetLoadedDelegate();

	public event OnAllAssetLoadedDelegate OnAllAssetLoadedEvent;

	public interface IRegistration
	{
		void RegisterAsset(DungeonAssetsRegister register);
		void LoadAsset(AssetMgr assetMgr, OnAssetLoadedDelegate callback);
	}

	[Inject]
	public AssetMgr assetMgr { get; set; }

	[Inject(DungeonContext.loadingProgressBar)]
	public GameObject loadingProgressBar { get; set; }

	private Dictionary<string, Type> assetsToRegister = new Dictionary<string, Type>();
	private Dictionary<string, object> cachedAssets = new Dictionary<string, object>();

	private int loadedAssetCount = 0;
	private UISlider loadingSlider;

	public void Register(string assetPath, Type t)
	{
		assetsToRegister.Add(assetPath, t);
	}

	public void LoadAllAssets()
	{
		MethodInfo methodInfo = typeof(AssetMgr).GetMethod("GetAsset", BindingFlags.Public);
		for (int i = 0; i < assetsToRegister.Count; i++)
		{
			KeyValuePair<string, Type> item = assetsToRegister.ElementAt(i);
			string assetPath = item.Key;
			Type type = item.Value;

			Delegate del = Delegate.CreateDelegate(type, this, "OnAssetLoaded");
			methodInfo = methodInfo.MakeGenericMethod(type);
			methodInfo.Invoke(assetMgr, new object[] {assetPath, del});
		}
	}

	public void OnAssetLoaded()
	{
		loadedAssetCount++;
		if (loadingSlider == null)
		{
			loadingSlider = loadingProgressBar.GetComponent<UISlider>();
		}
		loadingSlider.value = (float)loadedAssetCount / assetsToRegister.Count;
		if (loadedAssetCount < assetsToRegister.Count) return;
		if (OnAllAssetLoadedEvent != null)
		{
			OnAllAssetLoadedEvent();
		}
	}

	private void OnAssetLoaded<T>(T obj)
	{
		//Logger.Trace(GetType(), "::OnAssetLoaded");
	}

	public void CacheAsset(string assetPath, object obj)
	{
		cachedAssets.Add(assetPath, obj);
	}

	public T GetCachedAsset<T>(string assetPath)
	{
		return (T) cachedAssets[assetPath];
	}
}