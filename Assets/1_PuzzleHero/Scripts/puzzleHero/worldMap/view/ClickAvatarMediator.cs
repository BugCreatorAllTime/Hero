using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using strange.extensions.mediation.impl;

public class ClickAvatarMediator : Mediator {

	[Inject]
	public ClickAvatarView click{get; set;}

	public override void OnRegister ()
	{
		base.OnRegister ();
		click.clickSignal.AddListener(OnClickAvatar);
	}

	void OnClickAvatar(string name, int gender, List<int> equipments){
		Vector3 pos = gameObject.transform.localPosition + gameObject.transform.parent.localPosition +
						gameObject.transform.parent.parent.localPosition;
		GameObject.Find ("Tab").GetComponent<LoadInfoTab> ().ShowInfoFBDetail (pos.x, pos.y, name, gender, equipments, gameObject.transform, click);
	}
}
