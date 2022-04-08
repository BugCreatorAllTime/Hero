using UnityEngine;
using System.Collections;

/// <summary>
/// Tile cell effect and touch/click event.
/// </summary>
public class MatchItem : MonoBehaviour {

	private const int flagBeingAffected = 1;					//0000 0001
	private const int flagBeingLocked = 2;						//0000 0010

	private const int maskNonAffectable = 1;					//0000 0001
	private const int maskNonSwappable = 2;						//0000 0010

	public GameObject target;
	public Cell cell;
	public TilePoint point;
	GameObject[] flashPrefab;
	UISprite sprite;
	private int flag = 0;

	public bool Swappable()
	{
		return !HasAny(maskNonSwappable);
	}

	public void SetLocked(bool locked)
	{
		SetOrClearFlag(flagBeingLocked, locked);
	}

	public void SetAffected(bool affected)
	{
		SetOrClearFlag(flagBeingAffected, affected);
	}

	public bool IsBeingAffected()
	{
		return HasAny(maskNonAffectable);
	}

	public bool IsAffectable()
	{
		return !HasAny(maskNonAffectable);
	}

	private void SetOrClearFlag(int flag, bool setFlag) {
		if (setFlag) {
			SetFlag(flag);
		}
		else {
			ClearFlag(flag);
		}
	}

	private void SetFlag(int flag) {
		this.flag |= flag;
	}

	private void ClearFlag(int flag) {
		this.flag &= ~flag;
	}

	private bool HasAny(int mask)
	{
		return (mask & flag) != 0;
	}

	void Start()
	{
		sprite = GetComponent<UISprite>();
	}

//	void Update()
//	{
//		if (label == null)
//		{
//			label = GetComponentInChildren<UILabel>();
//		}
//		label.text = flag.ToString();
//	}
//
//	UILabel label;

	void OnSwapMotionComplete()
	{
		sprite.depth = 1;
	}
}
