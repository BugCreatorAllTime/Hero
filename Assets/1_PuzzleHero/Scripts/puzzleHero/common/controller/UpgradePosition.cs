using UnityEngine;
using System.Collections;

public class UpgradePosition : MonoBehaviour {

	private Spine.Bone bone ;
	public string boneName;
	public ItemBaseData item;
	public Transform child;
	private const int ASSASSIN_WEAPON = 1009;
	private int add = 0;
	// Use this for initialization
	void Start () {
		bone = gameObject.transform.parent.GetComponent<SkeletonAnimation>().skeleton.FindBone (boneName);

	}

	public void ResetPos()
	{
		if(item.Id == ASSASSIN_WEAPON && item.GetSlot() == ItemCfg.WEAPON_SLOT)
		{
			add = -30;
			child.localPosition = new Vector3 (30,-5,0);
		} else if(item.GetSlot() == ItemCfg.WEAPON_SLOT){
			add = 0;
			child.localPosition = new Vector3 (65,-5,0);
		}
	}

	// Update is called once per frame
	void Update () {
		transform.localPosition = new Vector3(bone.WorldX, bone.WorldY, -1f);
		transform.localRotation = Quaternion.Euler(new Vector3(0, 0, bone.WorldRotation+90+add));
	}
}
