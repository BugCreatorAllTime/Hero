using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;

public class CreateCloudMediator : Mediator 
{	
	[Inject]
	public CreateCloudView view {get; set;}
	
	public override void OnRegister ()
	{
		base.OnRegister ();
		view.Init();
	}
}
