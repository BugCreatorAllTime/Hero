using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;

public class GoToHouseMediator : Mediator {

	[Inject]
	public ClickGoToHouseView click{get; set;}
	[Inject]
	public GoToHouseManager goHouseManager{ get; set; }

	public override void OnRegister ()
	{
		base.OnRegister ();
		click.clickSignal.AddListener(GoHouse);
	}

	void GoHouse(string nameHouse)
	{
		goHouseManager.GoHouse (nameHouse);

	}
}
