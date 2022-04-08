using UnityEngine;
using System.Collections;

public class ScaleScreen : MonoBehaviour {

	public float x;
	public float width;
	public float height;

	// Use this for initialization
	void Start () {
		transform.localPosition = new Vector3 (x * (((float)Screen.width / Screen.height) / (width / height)), transform.localPosition.y, 0);
	}

}
