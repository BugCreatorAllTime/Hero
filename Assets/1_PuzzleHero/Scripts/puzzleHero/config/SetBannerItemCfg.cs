using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SetBannerItemCfg {

	public Dictionary<string, SetBannerInfoData> set = new Dictionary<string, SetBannerInfoData>();
	
	public SetBannerInfoData getSetInfo (int id)
	{
		return this.set [id.ToString ()];
	}
}
