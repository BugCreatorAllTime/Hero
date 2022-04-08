using UnityEngine;
using strange.extensions.context.impl;

public class LoadingContext : MVCSContext {

	public LoadingContext (MonoBehaviour contextView) : base (contextView)
	{
	}

	protected override void mapBindings ()
	{
		base.mapBindings ();
		mediationBinder.Bind<HintView>().To<HintMediator>();
	}

	protected override void postBindings ()
	{
		base.postBindings ();
		LoadingManager loadManager = injectionBinder.GetInstance<LoadingManager>();
		loadManager.LoadScreen ();
	}

}
