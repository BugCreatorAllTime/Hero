using UnityEngine;
using System.Collections;
using GoPlaySDK;

public class DestroyProgress : MonoBehaviour {

	void Awake() {
		DontDestroyOnLoad(transform.gameObject);
	}

	public void DeleteAll()
	{
		foreach (GameObject o in Object.FindObjectsOfType<GameObject>()) {
			if(o.name != "UnityFacebookSDKPlugin" && o.name != "(singleton) GoPlaySDK.GoPlaySdk")
				Destroy(o);
		}
		Destroy (transform.gameObject);
	}
}
