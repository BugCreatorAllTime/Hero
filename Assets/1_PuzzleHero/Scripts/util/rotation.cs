using UnityEngine;
using System.Collections;

public class rotation : MonoBehaviour {


	public float rotationSpeed = 150;
	public Vector3 pivot = Vector3.forward;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(pivot * Time.deltaTime * rotationSpeed );
	}
}
