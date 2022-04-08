using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;

public class GenerateMapMediator : Mediator {

	[Inject]
	public GenMapView view {get; set;}

	public override void OnRegister ()
	{
		base.OnRegister ();
		view.Init();
	}
}
