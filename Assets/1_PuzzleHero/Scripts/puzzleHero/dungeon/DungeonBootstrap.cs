
using UnityEngine;
using Nfury.Base;
using strange.extensions.context.impl;

public class DungeonBootstrap : ContextView
{
	void Awake ()
	{
		context = new DungeonContext (this);
	}
	

}


