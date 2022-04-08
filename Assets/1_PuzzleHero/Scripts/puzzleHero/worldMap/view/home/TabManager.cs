using UnityEngine;
using System.Collections;

public class TabManager : MonoBehaviour {
	
	public GameObject[] tab;
	public GameObject[] tabContainer;
	public int indexTabChoice = 1;
	public string TABSIDE = "TabSide";
	public string TABBEHIND = "TabBehind";
	public string TABMID = "TabMid";

	private const string objectName = "BoardShop";
	private bool checkTrialItem = false;
	private LoadInfoShop loadShop;


	// Update is called once per frame
	void Start()
	{
		if (gameObject.name == objectName)
		{
			checkTrialItem = true;
			loadShop = gameObject.transform.parent.GetComponent<LoadInfoShop>();
		}
	}

	public void SetTab()
	{
		switch(indexTabChoice)
		{
		case 1:
			ChoiceTab(0);
			break;
		case 2:
			ChoiceTab(1);
			break;
		case 3:
			ChoiceTab(2);
			break;
		case 4:
			ChoiceTab(3);
			break;
		default:
			break;
		} 
		if (checkTrialItem)
						loadShop.RefreshTrialItem ();
	}

	void ChoiceTab(int index)
	{
		for(int i = 0; i < tab.Length; i++)
		{
			if(i == index)
			{
				tabContainer[i].SetActive(true);
				if(i == 0 || i == tabContainer.Length - 1)
					tab[i].transform.GetComponent<UIButton>().normalSprite = TABSIDE;
				else tab[i].transform.GetComponent<UIButton>().normalSprite = TABMID;
			} else {
				tabContainer[i].SetActive(false);
				tab[i].transform.GetComponent<UIButton>().normalSprite = TABBEHIND;
			}
		}
	}
}
