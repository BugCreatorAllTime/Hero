using UnityEngine;
using System.Collections;
using strange.extensions.signal.impl;
using strange.extensions.mediation.impl;

public class OscillateView : View {

	public const int VERTICALLY = 1;
	public const int HORIZONTAL = 2;
	public const int FLASH = 3;

	public int state;
	public Vector3 pos;
	public bool start;
	public int index;
	public bool left;
	public int numerical;
}
