using UnityEngine;
using System.Collections;

public class ClickTab : MonoBehaviour {

	public int indexTab;
	public string name;
	TabManager manager;
	// Use this for initialization
	void Start () {
		manager = GameObject.Find(name).GetComponent<TabManager>();
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void CliCkTab()
	{
		if(manager.indexTabChoice != indexTab)
		{
			manager.indexTabChoice = indexTab;
			manager.SetTab ();
		}
	}
}
