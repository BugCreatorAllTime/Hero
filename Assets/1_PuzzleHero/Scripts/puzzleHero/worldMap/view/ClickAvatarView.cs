using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using strange.extensions.signal.impl;
using strange.extensions.mediation.impl;

public class ClickAvatarView : View {

	internal Signal<string, int, List<int>> clickSignal = new Signal<string, int, List<int>>();
	public string name;
	public int gender;
	public List<int> equipments;
	public string iconName;

	public void Click(){
		if(equipments != null)
			clickSignal.Dispatch(name, gender, equipments);
	}

	public void ResetAva()
	{
		gameObject.GetComponent<UISprite> ().spriteName = iconName;
	}

	public void SetData(string name, int gender, List<int> equipments, string iconName)
	{
		this.name = name;
		this.gender = gender;
		this.equipments = equipments;
		this.iconName = iconName;
	}
}
