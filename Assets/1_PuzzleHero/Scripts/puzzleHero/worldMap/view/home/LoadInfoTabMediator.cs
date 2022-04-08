using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;

public class LoadInfoTabMediator : Mediator {

	[Inject]
	public LoadInfoTab loadTab { get; set;}

	public override void OnRegister ()
	{
		base.OnRegister ();
	}
}
