using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;

public class HintMediator : Mediator {

	[Inject]
	public HintView hintView { get; set;}
	
	public override void OnRegister ()
	{
		base.OnRegister ();
	}
}
