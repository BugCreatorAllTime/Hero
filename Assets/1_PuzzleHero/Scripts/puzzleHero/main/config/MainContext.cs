using System;
using strange.examples.strangerocks;
using UnityEngine;
using common.network;
using common.service;

public class MainContext : SignalContext
{
	public MainContext (MonoBehaviour contextView) : base (contextView)
	{
	}

	protected override void mapBindings ()
	{
		base.mapBindings ();
    	Application.targetFrameRate = 60;

		// for Services
		injectionBinder.Bind<IRoutineRunner> ().To<RoutineRunner> ().ToSingleton ().CrossContext ();
		injectionBinder.Bind<IPaymentService>().To<GoPlayPaymentService>().ToSingleton().CrossContext();
#if UNITY_IOS
//		injectionBinder.Bind<IPaymentService>().To<AppStorePaymentService>().ToSingleton().CrossContext();
		injectionBinder.Bind<NotificationService> ().To<IOSNotifService>().ToSingleton ().CrossContext ();
#elif UNITY_ANDROID
//		injectionBinder.Bind<IPaymentService>().To<GooglePlayPaymentService>().ToSingleton().CrossContext();
		injectionBinder.Bind<NotificationService>().To<AndroidNotifService>().ToSingleton().CrossContext();
#endif
			injectionBinder.Bind<ApplicationDispatcherService> ().ToSingleton ().CrossContext ();
			injectionBinder.Bind<HttpConnection> ().ToSingleton ().CrossContext ();
			injectionBinder.Bind<DataService> ().ToSingleton ().CrossContext ();
			injectionBinder.Bind<ServerService> ().ToSingleton ().CrossContext ();
			injectionBinder.Bind<ItemService> ().ToSingleton ().CrossContext ();
			injectionBinder.Bind<ShopService> ().ToSingleton ().CrossContext ();
			injectionBinder.Bind<ChestService> ().ToSingleton ().CrossContext ();
			injectionBinder.Bind<DungeonService> ().ToSingleton ().CrossContext ();
			injectionBinder.Bind<AssetMgr> ().ToSingleton ().CrossContext ();
			injectionBinder.Bind<StartSignal> ().ToSingleton ();
			injectionBinder.Bind<OnLoginSignal> ().ToSingleton ();
			injectionBinder.Bind<InfoUserService> ().ToSingleton ().CrossContext ();
			injectionBinder.Bind<SoundManager> ().ToSingleton ().CrossContext ();
			injectionBinder.Bind<FbHandler>().ToSingleton().CrossContext();
			injectionBinder.Bind<GA>().ToSingleton().CrossContext();

            injectionBinder.Bind<ConfigManager>().ToSingleton().CrossContext();
            injectionBinder.Bind<CrossContextData>().ToSingleton().CrossContext();
			injectionBinder.Bind<LoadingManager> ().ToSingleton ().CrossContext ();

			injectionBinder.Bind<MainScreenHandler>().ToSingleton();
			injectionBinder.Bind<GoPlayFlushData> ().ToSingleton ().CrossContext ();

			mediationBinder.Bind<MainScreenView>().To<MainScreenMediator>();
			mediationBinder.Bind<GTokenView>().To<GTokenMediator>();
			mediationBinder.Bind<NoticeMainView>().To<NoticeMainMediator>();
			mediationBinder.Bind<LoadingView>().To<LoadingMediator>();

			commandBinder.Bind<UpgradeFxSignal> ().To<UpgradeFxCommand> ();
			commandBinder.Bind<GoToDungeonTutorialSignal>().To<GoToDungeonTutorialCommand>();
			commandBinder.Bind<ShowInfoFbSignal> ().To<ShowInfoFbCommand> ();
			commandBinder.Bind<StartSignal>().To<StartMainCommand>().Once();
			commandBinder.Bind<PlaySignal>().To<PlayCommand>().Once();
		}

		protected override void postBindings ()
		{
			base.postBindings ();

		}

	public override void Launch()
	{
		base.Launch();

		AssetMgr assetMgr = injectionBinder.GetInstance<AssetMgr>();
		GameObject obj;

		assetMgr.GetAsset<GameObject>("Prefabs/GUI/MainScreen/MainScreen", delegate(GameObject go)
		{
			obj = GameObject.Instantiate(go) as GameObject;
			obj.name = MainScreenObjectName.mainScreen;
			injectionBinder.Bind<GameObject>().ToValue(obj).ToName(MainScreenObjectName.mainScreen);
			obj.AddComponent<MainScreenView>();
		});

		assetMgr.GetAsset<GameObject>("Prefabs/GUI/MainScreen/UIGToken", delegate(GameObject go)
		                              {
			obj = GameObject.Instantiate(go) as GameObject;
			obj.name = MainScreenObjectName.gToken;
			injectionBinder.Bind<GameObject>().ToValue(obj).ToName(MainScreenObjectName.gToken);
			obj.AddComponent<GTokenView>();
		});

		assetMgr.GetAsset<GameObject>("Prefabs/GUI/MainScreen/PopUpNotice", delegate(GameObject go)
		                              {
			obj = GameObject.Instantiate(go) as GameObject;
			obj.name = MainScreenObjectName.notice;
			injectionBinder.Bind<GameObject>().ToValue(obj).ToName(MainScreenObjectName.notice);
			obj.AddComponent<NoticeMainView>();
		});

		assetMgr.GetAsset<GameObject>("Prefabs/GUI/Loading", delegate(GameObject go)
		                              {
			obj = GameObject.Instantiate(go) as GameObject;
			obj.name = MainScreenObjectName.loading;
			injectionBinder.Bind<GameObject>().ToValue(obj).ToName(MainScreenObjectName.loading);
			obj.AddComponent<LoadingView>();
		});
	}
}


