using UnityEngine;
using System.Collections;
using Nfury.Base;

public class GameInput : MonoBehaviour {

	public delegate void Dragged (DragGesture gesture);
	public event Dragged Drag;

	public delegate void FingerDowned(FingerDownEvent fingerEvent);
	public event FingerDowned FingerDown;

	public delegate void FingerUp(FingerUpEvent fingerEvent);
	public event FingerUp FingerUpEvent;

	public void OnDrag(DragGesture gesture){
		if(Drag != null) {
			Drag(gesture);
		}
	}

	public void OnFingerDown(FingerDownEvent fingerEvent)
	{
		if (FingerDown != null)
		{
			FingerDown(fingerEvent);
		}
	}

	public void OnFingerUp(FingerUpEvent fingerEvent)
	{
		if (FingerUpEvent != null)
		{
			FingerUpEvent(fingerEvent);
		}
	}
}
