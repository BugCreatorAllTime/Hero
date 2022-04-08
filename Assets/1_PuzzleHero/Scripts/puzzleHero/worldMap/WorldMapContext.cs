using strange.extensions.context.impl;
using UnityEngine;
using System.Collections;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using System;
using System.Reflection;
using strange.extensions.pool.api;
using strange.extensions.pool.impl;
using strange.examples.strangerocks;
using System.Collections.Generic;
using System.Linq;

public class WorldMapContext : MVCSContext
{

	public const string GRID = "Grid";
	private Dictionary<GameObject, Type> viewList = new Dictionary<GameObject, Type> (); 
	private int count = 0;

	public WorldMapContext(MonoBehaviour view)
		: base(view)
	{
	}



	protected override void postBindings()
	{
		base.postBindings();
		AssetMgr assetMgr = injectionBinder.GetInstance<AssetMgr>();
		LoadInfoTab tab = GameObject.Find("Tab").GetComponent<LoadInfoTab>();
		injectionBinder.Bind<LoadInfoTab>().ToValue(tab);
		Type type = typeof(PrefabWorldMap);
		foreach (FieldInfo fieldInfo in type.GetFields())
		{
			object v = fieldInfo.GetValue(null);
			String name = v.ToString();
			IPool<GameObject> pool = injectionBinder.GetInstance<IPool<GameObject>>(name);
			pool.instanceProvider = new ResourceInstanceProvider(name, assetMgr);
			pool.inflationType = PoolInflationType.INCREMENT;
		}

		GameObject obj;
		GameObject buttonPanel = GameObject.Find("ButtonPanel");
		obj = GameObject.Find(WorldMapObjectName.settingsButton);
		obj.name = WorldMapObjectName.settingsButton;
		injectionBinder.Bind<GameObject>().ToValue(obj).ToName(WorldMapObjectName.settingsButton);
		obj.AddComponent<SettingsButtonView>();
		assetMgr.GetAsset<GameObject> ("Prefabs/WorldMap/SettingsPopup", delegate (GameObject go){
			obj.SetActive(true);
			obj = GameObject.Instantiate(go) as GameObject;
			obj.transform.parent = buttonPanel.transform;
			obj.transform.localScale = Vector3.one;
			obj.name = WorldMapObjectName.settingsPopup;
			injectionBinder.Bind<GameObject>().ToValue(obj).ToName(WorldMapObjectName.settingsPopup);
			viewList.Add(obj, typeof(SettingsPopupView));
			obj.SetActive(false);
//			obj.AddComponent<SettingsPopupView>();
			OnViewLoaded(buttonPanel);
		});
		assetMgr.GetAsset<GameObject> ("Prefabs/WorldMap/NotifyFb", delegate (GameObject go){
			obj.SetActive(true);
			obj = GameObject.Instantiate(go) as GameObject;
			obj.transform.parent = buttonPanel.transform;
			obj.transform.localScale = Vector3.one;
			obj.name = WorldMapObjectName.notifyFB;
			injectionBinder.Bind<GameObject>().ToValue(obj).ToName(WorldMapObjectName.notifyFB);
			viewList.Add(obj, typeof(NotifyFBView));
			obj.SetActive(false);
//			obj.AddComponent<NotifyFBView>();
			OnViewLoaded(buttonPanel);
		});		
	}

	private void OnViewLoaded(GameObject buttonPanel)
	{
		count++;
		if(count == 2)
		{
			for(int i = 0; i < viewList.Count; i++)
			{
				viewList.ElementAt(i).Key.SetActive(true);
				viewList.ElementAt(i).Key.AddComponent(viewList.ElementAt(i).Value);
			}
			injectionBinder.GetInstance<NotifyFbSignal>().Dispatch();
		}
	}

	protected override void addCoreComponents()
	{
		base.addCoreComponents();
		injectionBinder.Unbind<ICommandBinder>();
		injectionBinder.Bind<ICommandBinder>().To<SignalCommandBinder>().ToSingleton();
	}

	protected override void mapBindings()
	{
		base.mapBindings();

		injectionBinder.Bind<GoToHouseManager>().ToSingleton();
		injectionBinder.Bind<ItemChestEffectManager>().ToSingleton();
		injectionBinder.Bind<TutorialService> ().ToSingleton ();
		injectionBinder.Bind<InfoFbManager>().ToSingleton();
		injectionBinder.Bind<ClickTutManager> ().ToSingleton ();
		injectionBinder.Bind<GameObject>().ToValue(GameObject.Find("Grid")).ToName(WorldMapContext.GRID);
		injectionBinder.Bind<WorldMapHander>().ToSingleton();
		injectionBinder.Bind<NoteDungeonManager> ().ToSingleton ();

		commandBinder.Bind<GoToDungeonSignal>().To<GoToDungeonCommand>().To<SaveGameCommand>().InSequence();
		commandBinder.Bind<NotifyFbSignal>().To<NotifyFbCommand>();

		mediationBinder.Bind<LoadInfoTab>().To<LoadInfoTabMediator>();
		mediationBinder.Bind<GenMapView>().To<GenerateMapMediator>();
		mediationBinder.Bind<ClickDungeonView>().To<ClickMediator>();
		mediationBinder.Bind<ClickGoToHouseView>().To<GoToHouseMediator> ();
		mediationBinder.Bind<OscillateView>().To<OscillateMediator> ();
		mediationBinder.Bind<CreateCloudView> ().To<CreateCloudMediator> ();
		mediationBinder.Bind<ClickBackHomeTownView> ().To<ClickBackHomeTownMediator> ();
		mediationBinder.Bind<SettingsButtonView>().To<SettingsButtonMediator>();
		mediationBinder.Bind<SettingsPopupView>().To<SettingsPopupMediator>();
		mediationBinder.Bind<NotifyFBView>().To<NotifyFBMediator>();
		mediationBinder.Bind<ClickAvatarView>().To<ClickAvatarMediator>();

		Type type = typeof(PrefabWorldMap);
		foreach (FieldInfo fieldInfo in type.GetFields())
		{
			object field = fieldInfo.GetValue(null);
			injectionBinder.Bind<IPool<GameObject>>().To<Pool<GameObject>>().ToSingleton().ToName(field.ToString());
		}
		GameObject gameObject = GameObject.FindWithTag(ObjectTag.gameInput);
		GameInput gameInput = gameObject.GetComponent<GameInput>();
		injectionBinder.Bind<GameInput>().ToValue(gameInput).ToSingleton();

	}

}
