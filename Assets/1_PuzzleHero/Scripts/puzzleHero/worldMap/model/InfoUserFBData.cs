using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InfoUserFBData {

	public InfoUserFBData(int currentLevel, UIAtlas atlas, string iconName):this(currentLevel, atlas, iconName, "", 1, null)
	{

	}

	public InfoUserFBData (int currentLevel, UIAtlas atlas, string iconName, string name, int gender, List<int> equipments)
	{
		this.currentLevel = currentLevel;
		this.atlas = atlas;
		this.iconName = iconName;
		this.name = name;
		this.gender = gender;
		this.equipments = equipments;
	}

	public int currentLevel { get; set;}
	public UIAtlas atlas { get; set;}
	public string iconName {get ; set;}
	public int gender { get; set;}
	public string name { get; set;}
	public List<int> equipments { get; set;}
}
