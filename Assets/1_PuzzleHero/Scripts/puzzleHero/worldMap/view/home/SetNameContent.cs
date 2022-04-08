using UnityEngine;
using System.Collections;

public class SetNameContent : MonoBehaviour {

	public UISprite arrow;
	public UISprite disable;
	public UIButton accept;
	public UILabel name;
	public UISprite character;
	public UISprite bg;
	public UIButton maleSelect;
	public UIButton femaleSelect;
	public Camera camera;
	public UILabel number;
	public UIPanel show;
	public UIButton close;
	public UIPanel list;
	public UISprite talk;
	public UILabel textTalk;
	public UILabel skip;

	private string textShow = "";
	private int i = -1;
	private float time = 0;
	public bool flagTalk = false;
	private float alpha = 1;
	private int direction = 1;
	private int directionPos = 1;
	private const float TIME_TO_TALK_ONE_WORD = 0.015f;
	private Vector3 posArrow;
	private float y;

	public void PreTalk()
	{
		textShow = textTalk.text;
		textTalk.text = "";
		i = -1;
		time = 0;
		flagTalk = false;
		if(skip != null)
			skip.gameObject.SetActive (true);
		if(arrow != null && arrow.gameObject.activeInHierarchy)
		{
			posArrow = arrow.transform.localPosition;
			y = 0;
		}
	}

	public void Talk()
	{
		flagTalk = true;
	}

	void Update()
	{
		if(flagTalk)
		{
			if(i < textShow.Length - 1)
			{
				time += Time.deltaTime;
				if(time >= TIME_TO_TALK_ONE_WORD)
				{
					time = 0;
					i++;
					textTalk.text += textShow[i];
				}
			} else {
				flagTalk = false;
			}
		}
		if(skip != null && skip.gameObject.activeInHierarchy)
		{
			skip.alpha = alpha;
			if(alpha >= 1 && direction == 1)direction = -1;
			if(alpha <= 0.3f && direction == -1) direction = 1;
			alpha += Time.deltaTime * direction;
		}

		if(arrow != null && arrow.gameObject.activeInHierarchy)
		{
			if(arrow.transform.localPosition.y >= posArrow.y+15 && directionPos == 1)directionPos = -1;
			if(arrow.transform.localPosition.y <= posArrow.y-15 && directionPos == -1) directionPos = 1;
			y += 60 * Time.deltaTime * directionPos;
			arrow.transform.localPosition = new Vector3(posArrow.x,posArrow.y+y,posArrow.z);
		}
	}

	public void TalkComplete()
	{
		flagTalk = false;
		if(textTalk.text != null && textShow != null)
			textTalk.text = textShow;
	}
}
