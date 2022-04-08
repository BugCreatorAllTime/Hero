using UnityEngine;
using System.Collections;

public class ScaleBg : MonoBehaviour {

	public float width;
	public float height;

	// Use this for initialization
	void Start () {
		Vector3 scale = transform.localScale;
		transform.localScale = new Vector3 (scale.x * (((float)Screen.width / Screen.height) / (width / height)),scale.y,scale.z);
	}

}
