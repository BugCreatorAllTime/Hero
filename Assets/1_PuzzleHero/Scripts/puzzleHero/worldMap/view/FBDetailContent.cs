using UnityEngine;
using System.Collections;

public class FBDetailContent : MonoBehaviour {

	public UILabel name;
	public Camera camera;
	public UIButton button;
	public UISprite content;
	public UISprite disable;
	public SkeletonAnimation ske;
	public ClickAvatarView view;
	private float time;

	void Update()
	{
		time += Time.deltaTime;
		if(time >= 5)
		{
			time = 0;
			int r = Random.Range(1,4);
			switch(r)
			{
			case 1:
				Attack();
				break;
			case 2:
				BeHit();
				break;
			case 3:
				CastSkill();
				break;
			default:
				break;
			}
		}
	}

	void Attack()
	{
		ske.state.SetAnimation(0, AnimationName.Attack, false);
		ske.state.AddAnimation (0, AnimationName.Idle, true,0);
	}

	void BeHit()
	{
		ske.state.SetAnimation(0, AnimationName.BeHit, false);
		ske.state.AddAnimation (0, AnimationName.Idle, true,0);
	}

	void CastSkill()
	{
		ske.state.SetAnimation(0, AnimationName.Cast, false);
		ske.state.AddAnimation (0, AnimationName.Idle, true,0);
	}
}
