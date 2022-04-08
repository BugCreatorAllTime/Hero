using UnityEngine;
using System.Collections;
using strange.extensions.signal.impl;
using strange.extensions.mediation.impl;

public class ClickDungeonView : View
{
	public const string NODE = "nodemap";
	public const string LOCK = "_lock";
	public const string UNLOCK = "_unlock";
	public UIButton bt;
	public UISprite engery;
	public UILabel lb_energy;
	public UILabel lb_id;
	public BoxCollider collider;
	public UISprite iconChest;
	public UISprite ava;

	private int id;
	internal Signal<int> clickSignal = new Signal<int>();
    
	protected override void Awake ()
	{
		UIEventListener.Get(GetComponent<UIButton>().gameObject).onPress += PressButton;
	}

    public void Click(){
		clickSignal.Dispatch(id);
    }

	public void SetData(int id, int mapNum, int energy, int levelReq, bool isLock)
	{
		string spriteName = NODE + mapNum + (isLock?LOCK:UNLOCK);
		bt.normalSprite = spriteName;
		lb_energy.text = (energy.ToString());
		lb_id.text = id.ToString();
		collider.enabled = !isLock;
		this.id = id;
	}

	void PressButton(GameObject go, bool isPressed)
	{
		if(isPressed)
		{
			gameObject.transform.localScale = new Vector3(1.25f,1.25f,1.25f);
		} else {
			gameObject.transform.localScale = Vector3.one;
		}
	}
}
