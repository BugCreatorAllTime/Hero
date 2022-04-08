using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundCfg {

	public Dictionary<string, SoundData> sound = new Dictionary<string, SoundData>();
	
	public SoundData getSound (int id)
	{
		return this.sound [id.ToString ()];
	}
}
