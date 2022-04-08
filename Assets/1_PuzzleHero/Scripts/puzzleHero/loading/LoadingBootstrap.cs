using UnityEngine;
using System.Collections;
using strange.extensions.context.impl;

public class LoadingBootstrap : ContextView {

	void Awake ()
	{
		context = new LoadingContext (this);
	}
}
